using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using BobDono.Contexts;
using BobDono.Core;
using BobDono.Core.Attributes;
using BobDono.Core.Extensions;
using BobDono.Core.Interfaces;
using BobDono.Core.Utils;
using BobDono.Interfaces;
using BobDono.Interfaces.Services;
using BobDono.Models.Entities;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace BobDono.Modules
{
    [Module(Name = "Matchups",Description = "Allows to create matchups.")]
    public class MatchupModule
    {
        private readonly CustomDiscordClient _customDiscordClient;

        private readonly IServiceFactory<IMatchupService> _matchupService;
        private readonly IServiceFactory<IUserService> _userService;

        private readonly IBotContext _botContext;
        private readonly IExceptionHandler _exceptionHandler;

        public List<MatchupContext> MatchupContexts { get; set; } = new List<MatchupContext>();

        public MatchupModule(IServiceFactory<IUserService> userService, CustomDiscordClient customDiscordClient,
            IServiceFactory<IMatchupService> matchupService, IBotContext botContext, IExceptionHandler exceptionHandler)
        {
            _userService = userService;
            _customDiscordClient = customDiscordClient;
            _matchupService = matchupService;
            _botContext = botContext;
            _exceptionHandler = exceptionHandler;

            InitializeExistingElections();
        }

        private void InitializeExistingElections()
        {
            using (var dependencyScope = ResourceLocator.ObtainScope())
            using (var electionService = _matchupService.ObtainLifetimeHandle(ResourceLocator.ExecutionContext))
            {
                foreach (var matchup in electionService.GetAll()
                    .Where(matchup => matchup.CurrentState < Matchup.State.Finished))
                {
                    var guild = ResourceLocator.DiscordClient.GetNullsGuild();
                    var channel = _customDiscordClient.GetChannel(guild, (ulong)matchup.DiscordChannelId);
                    if (channel != null)
                    {
                        MatchupContexts.Add(
                            dependencyScope.Resolve<MatchupContext>(new TypedParameter(typeof(Matchup), matchup),
                                new TypedParameter(typeof(DiscordChannel), channel)));
                    }
                    else
                    {
                        matchup.CurrentState = Matchup.State.ClosedForcibly;
                    }                
                }
            }
        }

        [CommandHandler(Regex = "matchup create",Awaitable = false,HumanReadableCommand = "matchup create",HelpText = "Creates new matchup.")]
        public async Task CreateNewMatchup(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            using (var userService = _userService.ObtainLifetimeHandle(executionContext))
            using (var matchupService = _matchupService.ObtainLifetimeHandle(executionContext))
            {
                if ((await matchupService.GetAllWhereAsync(e => e.CurrentState < Matchup.State.Finished)).Count >= 2)
                {
                    await args.Channel.SendMessageAsync(
                        "There are already 2 ongoing matchups. Please wait until one of them closes and try again.");
                    return;
                }

                var user = await userService.GetOrCreateUser(args.Author);


                var cts = new CancellationTokenSource();
                var timeout = TimeSpan.FromMinutes(3);
                var guild = ResourceLocator.DiscordClient.GetNullsGuild();
                var member = await guild.GetMemberAsync(args.Author.Id);
                var channel = await member.CreateDmChannelAsync();
                await channel.SendMessageAsync(
                    "You are about to create new matchup, you can always cancel by typing `quit`.\n");

                var matchup = new Matchup();
                try
                {
                    _botContext.NewPrivateMessage += HandleQuit;

                    try
                    {

                        matchup.Name = await channel.GetNextValidResponse<string>(
                            "Provide short name for it (2 characters+, alphanumeric). It will be used as channel name.",
                            async s =>
                            {
                                s = s.Replace(" ", "-");
                                if (s.Length >= 2 && Regex.IsMatch(s, "^[a-zA-Z0-9_-]*$"))
                                    return s;
                                return null;
                            }, timeout, cts.Token);
                        matchup.Description = await channel.GetNextValidResponse(
                            "Longer description if you could (500 characters):",
                            async s =>
                            {
                                if (s.Length <= 500)
                                    return s;
                                return null;
                            }, timeout, cts.Token);
                        int signupDays = 0;
                        while (signupDays == 0)
                        {
                            await channel.SendMessageAsync(
                                "How long would you like the sign-up to be? (1-7) days");
                            var response = await channel.GetNextMessageAsync(timeout, cts.Token);
                            if (int.TryParse(response, out int days))
                            {
                                if (days >= 1 && days <= 7)
                                {
                                    signupDays = days;
                                }
                            }
                        }
                        matchup.SignupsEndDate = DateTime.UtcNow.AddDays(signupDays);

                        int matchupDuration = 0;
                        while (matchupDuration == 0)
                        {
                            await channel.SendMessageAsync(
                                "How long would you like the matchup to last? (1-31) days");
                            var response = await channel.GetNextMessageAsync(timeout, cts.Token);
                            if (int.TryParse(response, out int count))
                            {
                                if (count >= 1 && count <= 31)
                                {
                                    matchupDuration = count;
                                }
                            }
                        }

                        matchup.ChallengesEndDate = matchup.SignupsEndDate.AddDays(matchupDuration);

                        await channel.SendMessageAsync(
                            "I'll go ahead and create channel for this matchup!");

                        var category =
                            await guild.GetCategoryChannel(DiscordClientExtensions.ChannelCategory.Matchups);
                        var electionChannel = await guild.CreateChannelAsync(matchup.Name, ChannelType.Text,
                            category);
                        matchup.DiscordChannelId = (long)electionChannel.Id;
                        _customDiscordClient.CreatedChannels.Add(electionChannel);


                        matchup.Author = user;
                        matchupService.Add(matchup);
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

                try
                {
                    var ch = _customDiscordClient.GetChannel(guild, (ulong)matchup.DiscordChannelId);
                    if (ch != null)
                    {
                        using (var dependencyScope = ResourceLocator.ObtainScope())
                        {
                            var matchupContext = dependencyScope.Resolve<MatchupContext>(new TypedParameter(
                                typeof(Matchup), matchup), new TypedParameter(typeof(DiscordChannel), ch));
                            matchup.OpeningMessageId = await matchupContext.OnCreate(matchup);
                            MatchupContexts.Add(matchupContext);
                        }
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
    }
}
