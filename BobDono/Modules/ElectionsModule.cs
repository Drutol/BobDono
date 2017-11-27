using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Autofac.Core;
using BobDono.Contexts;
using BobDono.Core;
using BobDono.Core.Attributes;
using BobDono.Core.BL;
using BobDono.Core.Extensions;
using BobDono.Core.Interfaces;
using BobDono.Core.Utils;
using BobDono.DataAccess.Database;
using BobDono.DataAccess.Services;
using BobDono.Interfaces;
using BobDono.Interfaces.Services;
using BobDono.Models.Entities;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;

namespace BobDono.Modules
{
    [Module(Name = "Elections", Description = "Allows to create new election, and view their overviews and such.")]
    public class ElectionsModule
    {
        private readonly IUserService _userService;
        private readonly IBotContext _botContext;
        private readonly IExceptionHandler _exceptionHandler;
        private readonly IElectionService _electionService;
        public List<ElectionContext> ElectionsContexts { get; } = new List<ElectionContext>();

        public ElectionsModule(IUserService userService, IBotContext botContext, IExceptionHandler exceptionHandler,
            IElectionService electionService)
        {
            _userService = userService;
            _botContext = botContext;
            _exceptionHandler = exceptionHandler;
            _electionService = electionService;

            InitializeExistingElections();
        }

        private void InitializeExistingElections()
        {
            using (var dependencyScope = ResourceLocator.ObtainScope())
            using (var electionService =
                _electionService.ObtainLifetimeHandle(ResourceLocator.ExecutionContext))
            {
                foreach (var election in electionService.GetAll()
                    .Where(election => election.CurrentState < Election.State.Closed))
                {
                    try
                    {
                        ElectionsContexts.Add(
                            dependencyScope.Resolve<ElectionContext>(new TypedParameter(typeof(Election),
                                election)));
                    }
                    catch (Exception) //couldn't create election -> channel removed
                    {
                        //we will mark is as closed
                        election.CurrentState = Election.State.ClosedForcibly;
                    }
                }
            }
        }

        [CommandHandler(
            Regex = @"election create",
            HumanReadableCommand = "election create",
            HelpText = "Starts new election.",
            Awaitable = false)]
        public async Task CreateElection(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            using (var userService = _userService.ObtainLifetimeHandle(executionContext))
            using (var electionService = _electionService.ObtainLifetimeHandle(executionContext))
            {
                electionService.ConfigureIncludes().IgnoreDefaultServiceIncludes().Commit();
                if ((await electionService.GetAllWhereAsync(e => e.CurrentState < Election.State.Closed)).Count >= 2)
                {
                    await args.Channel.SendMessageAsync(
                        "There are already 2 ongoing elections. Please wait until one of them closes and try again.");
                    return;
                }

                var user = await userService.GetOrCreateUser(args.Author);


                var cts = new CancellationTokenSource();
                var timeout = TimeSpan.FromMinutes(3);
                var guild = ResourceLocator.DiscordClient.GetNullsGuild();
                var member = await guild.GetMemberAsync(args.Author.Id);
                var channel = await member.CreateDmChannelAsync();
                await channel.SendMessageAsync(
                    "You are about to create new election, you can always cancel by typing `quit`.\n");

                var election = new Election();
                try
                {
                    _botContext.NewPrivateMessage += HandleQuit;

                    try
                    {

                        election.Name = await channel.GetNextValidResponse<string>(
                            "Provide short name for it (2 characters+, alphanumeric). It will be used as channel name.",
                            async s =>
                            {
                                s = s.Replace(" ", "-");
                                if (s.Length >= 2 && Regex.IsMatch(s, "^[a-zA-Z0-9_-]*$"))
                                    return s;
                                return null;
                            }, timeout, cts.Token);
                        election.Description = await channel.GetNextValidResponse(
                            "Longer description if you could (500 characters):",
                            async s =>
                            {
                                if (s.Length <= 500)
                                    return s;
                                return null;
                            }, timeout, cts.Token);
                        int submissionDays = 0;
                        while (submissionDays == 0)
                        {
                            await channel.SendMessageAsync(
                                "How long would you like the submission period to be? (1-7) days.");
                            var response = await channel.GetNextMessageAsync(timeout, cts.Token);
                            if (int.TryParse(response, out int days))
                            {
                                if (days >= 1 && days <= 7)
                                {
                                    submissionDays = days;
                                }
                            }
                        }
                        election.SubmissionsStartDate = DateTime.UtcNow;
                        election.SubmissionsEndDate = DateTime.Today.AddHours(election.SubmissionsStartDate.Hour + 1)
                            .AddDays(submissionDays);
                        election.VotingStartDate = election.SubmissionsEndDate.AddHours(2); //TODO Maybe add commands?

                        int submissionCount = 0;
                        while (submissionCount == 0)
                        {
                            await channel.SendMessageAsync(
                                "How many contestants can be submitted by one person? (1-9)");
                            var response = await channel.GetNextMessageAsync(timeout, cts.Token);
                            if (int.TryParse(response, out int count))
                            {
                                if (count >= 1 && count <= 9)
                                {
                                    submissionCount = count;
                                }
                            }
                        }
                        election.EntrantsPerUser = submissionCount;

                        await channel.SendMessageAsync(
                            "That'd be everything I guess, let the wars begin. I'll now create a new battlefield!");
                        try
                        {
                            var category =
                                await guild.GetCategoryChannel(DiscordClientExtensions.ChannelCategory.Elections);
                            var electionChannel = await guild.CreateChannelAsync(election.Name, ChannelType.Text,
                                category,
                                null, null, null,
                                election.Description);
                            election.DiscordChannelId = electionChannel.Id;
                            (ResourceLocator.DiscordClient as CustomDiscordClient).CreatedChannels.Add(electionChannel);
                        }
                        catch (Exception e)
                        {

                        }



                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }
                    catch (Exception e)
                    {
                        _exceptionHandler.Handle(e);
                    }

                }
                finally
                {
                    _botContext.NewPrivateMessage -= HandleQuit;
                }


                int retries = 0;
                while (true)
                {
                    var ch = ResourceLocator.DiscordClient.GetNullsGuild().GetChannel(election.DiscordChannelId);
                    if (ch != null)
                        break;
                    await Task.Delay(3000);
                    if (retries++ > 4)
                    {
                        await channel.SendMessageAsync("Something went wrong while obtaining discord channel.");
                        return;
                    }
                }
                try
                {
                    using (var dependencyScope = ResourceLocator.ObtainScope())
                    {
                        var electionContext = dependencyScope.Resolve<ElectionContext>(new TypedParameter(
                            typeof(Election),
                            await electionService.CreateElection(election, user)));
                        electionContext.OnCreated();
                        ElectionsContexts.Add(electionContext);
                    }
                }
                catch (Exception e)
                {

                }



                void HandleQuit(MessageCreateEventArgs a)
                {
                    if (a.Channel.Id == channel.Id &&
                        a.Message.Content.Equals("quit", StringComparison.CurrentCultureIgnoreCase))
                    {
                        cts.Cancel();
                    }
                }
            }
        }

        [CommandHandler(Regex = @"election list(\s<@\d+>|\s\w+|\s<@!\d+>|$)",
            HumanReadableCommand = "election list <username>", HelpText = "Lists all election related to given user.")]
        public async Task ListElection(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            using (var userService = _userService.ObtainLifetimeHandle(executionContext))
            {
                IQueryable<User> IncChain(IQueryable<User> query)
                {
                    return query.Include(u => u.Elections)
                        .Include(u => u.Votes)
                        .ThenInclude(v => v.Bracket)
                        .ThenInclude(b => b.BracketStage)
                        .ThenInclude(bs => bs.Election);
                }

                userService.ConfigureIncludes().WithChain(IncChain).Commit();


                User user = null;
                if (args.Message.GetSubject(out var username))
                {
                    user = await userService.FirstAsync(u => u.Name.ToLower().Contains(username.ToLower()));
                }
                else
                {
                    user = await userService.FirstAsync(u => u.DiscordId == args.Message.MentionedUsers.First().Id);
                }
  
                if (user == null)
                {
                    userService.ConfigureIncludes().WithChain(IncChain).Commit();
                    user = await userService.GetOrCreateUser(args.Message.Author);
                }

                var createdElections = user.Elections.Distinct(Election.IdComparer).ToList();
                var votedElections = user.Votes.Select(vote => vote.Bracket.BracketStage.Election)
                    .Distinct(Election.IdComparer).ToList();

                string output = null;
                if (createdElections.Any())
                {
                    output = "**Created elections**\n\n";
                    foreach (var createdElection in createdElections)
                        output += $"{createdElection.Name} - Id: {createdElection.Id}\n";
                }

                if (votedElections.Any())
                {
                    if (output == null)
                        output = "**Elections participated in**\n\n";
                    else
                        output += "\n**Elections participated in**\n\n";

                    foreach (var votedElection in votedElections)
                        output += $"{votedElection.Name} - Id: {votedElection.Id}\n";
                }

                if (output == null)
                    output = "User didn't participate in any elections yet.";

                await args.Channel.SendMessageAsync(output);
            }
        }

        [CommandHandler(Regex = @"election votes\s?\d{0,2}", HumanReadableCommand = "election votes [lastVotesCount]",
            HelpText =
                "Prints your last votes. By default it will list all your votes from election stage you particiapted most recently.")]
        public async Task ListVotes(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            var param = args.Message.Content.Split(' ');


            using (var userService = _userService.ObtainLifetimeHandle(executionContext))
            {
                userService.ConfigureIncludes().WithChain(q =>
                {
                    return q
                        .Include(u => u.Votes)
                            .ThenInclude(vote => vote.Bracket)
                            .ThenInclude(b => b.BracketStage)
                            .ThenInclude(b => b.Election)
                        .Include(u => u.Votes)
                            .ThenInclude(u => u.Contender)
                            .ThenInclude(c => c.Waifu);
                }).Commit();
                var user = await userService.GetOrCreateUser(args.Message.Author);
                if (user.Votes.Any())
                {
                    string output;
                    if (param.Length == 2) //last stage
                    {
                        var lastStage = user.Votes.Last().Bracket.BracketStage;
                        var votesInStage = user.Votes.Where(vote => vote.Bracket.BracketStage == lastStage);
                        output = $"**Votes in stage #{lastStage.Number} of {lastStage.Election.Name}**\n\n";
                        foreach (var vote in votesInStage)
                            output += $"{vote.Contender.Waifu.Name} *({vote.Contender.Waifu.MalId})* {(vote.Bracket.Winner != null && vote.Contender.Equals(vote.Bracket.Winner) ? ":trophy:" : "")}\n";
                    }
                    else //given amount
                    {
                        var votesToDisplay = int.Parse(param[2]);
                        var votesPerElection = user.Votes.TakeLast(votesToDisplay).GroupBy(vote => vote.Bracket.BracketStage.Election);
                        output = "";

                        foreach (var electionVotes in votesPerElection)
                        {
                            output += $"**Votes in {electionVotes.Key.Name} election:**\n\n";
                            foreach (var vote in electionVotes)
                                output += $"{vote.Contender.Waifu.Name} *({vote.Contender.Waifu.MalId})*  Stage #{vote.Bracket.BracketStage.Number} {(vote.Bracket.Winner != null && vote.Contender.Equals(vote.Bracket.Winner) ? ":trophy:" : "")}\n";
                            output += "\n";
                        }
                        
                    }

                    await args.Channel.SendMessageAsync(output);
                }
                else
                {
                    await args.Channel.SendMessageAsync(
                        "You didn't vote in any elections yet... I'm dissapointed but I believe I'll do better in the future!");
                }

            }


        }

    }
}
