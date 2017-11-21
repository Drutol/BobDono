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
    [Module(Name = "Elections",Description = "Allows to create new election, and view their overviews and such.")]
    public class ElectionsModule
    {
        private readonly IUserService _userService;
        private readonly IBotContext _botContext;
        private readonly IExceptionHandler _exceptionHandler;
        private readonly IElectionService _electionService;
        public List<ElectionContext> ElectionsContexts { get; } = new List<ElectionContext>();

        public ElectionsModule(IUserService userService,IBotContext botContext, IExceptionHandler exceptionHandler, IElectionService  electionService)
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

                        election.Name = await channel.GetNextValidResponse<string>("Provide short name for it (2 characters+, alphanumeric). It will be used as channel name.", async s =>
                        {
                            s = s.Replace(" ", "-");
                            if (s.Length >= 2 && Regex.IsMatch(s, "^[a-zA-Z0-9_-]*$"))
                                return s;
                            return null;
                        },timeout, cts.Token);
                        election.Description = await channel.GetNextValidResponse("Longer description if you could (500 characters):",
                            async s =>
                            {
                                if (s.Length <= 500)
                                    return s;
                                return null;
                            },timeout, cts.Token);
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
                        election.SubmissionsEndDate = DateTime.Today.AddHours(election.SubmissionsStartDate.Hour+1).AddDays(submissionDays);
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
                            var category = await guild.GetCategoryChannel(DiscordClientExtensions.ChannelCategory.Elections);
                            var electionChannel = await guild.CreateChannelAsync(election.Name, ChannelType.Text,
                                category,
                                null, null, null,
                                election.Description);
                            election.DiscordChannelId = electionChannel.Id;
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
                    if(ch != null)
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
                        var electionContext = dependencyScope.Resolve<ElectionContext>(new TypedParameter(typeof(Election),
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

        [CommandHandler(Regex = @"election list (<@\d+>|\w+|<@!\d+>)", HumanReadableCommand = "election list <username>",HelpText = "Lists all election related to given user.")]
        public async Task ListElection(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            using (var userService = _userService.ObtainLifetimeHandle(executionContext))
            {
                
                userService.ConfigureIncludes().WithChain(query =>
                {
                    return query.Include(u => u.Elections)
                        .Include(u => u.Votes)
                            .ThenInclude(v => v.Bracket)
                            .ThenInclude(b => b.BracketStage)
                                .ThenInclude(bs => bs.Election);
                }).Commit();


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
                    await args.Channel.SendMessageAsync("Couldn't find specified user.");
                    return;
                }

                var createdElections = user.Elections.Distinct(Election.IdComparer).ToList();
                var votedElections = user.Votes.Select(vote => vote.Bracket.BracketStage.Election).Distinct(Election.IdComparer).ToList();

                string output = null;
                if (createdElections.Any())
                {
                    output = "_Created elections_\n";
                    foreach (var createdElection in createdElections)
                        output += $"Created {createdElection.Name} - Id: {createdElection.Id}\n";
                }

                if (votedElections.Any())
                {
                    if (output == null)
                        output = "_Created elections_\n";
                    else
                        output += "_Created elections_\n";

                    foreach (var votedElection in votedElections)
                        output += $"Voted in {votedElection.Name} - Id: {votedElection.Id}\n";
                }

                if (output == null)
                    output = "User didn't participate in any elections yet.";

                await args.Channel.SendMessageAsync(output);
            }
        }
    }
}
