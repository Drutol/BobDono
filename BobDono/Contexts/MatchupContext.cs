using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BobDono.Controllers;
using BobDono.Core;
using BobDono.Core.Attributes;
using BobDono.Core.BL;
using BobDono.Core.Extensions;
using BobDono.Core.Utils;
using BobDono.Interfaces;
using BobDono.Interfaces.Services;
using BobDono.Models.Entities;
using BobDono.Models.Entities.JoinEntities;
using BobDono.Models.Entities.Simple;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;

namespace BobDono.Contexts
{
    [Module(IsChannelContextual = true,Name = "Matchup(Channel)")]
    public class MatchupContext : ContextModuleBase
    {
        public const string ParticipantsKey = "Participants";
        public const string SubmissionsUntilKey = "Entry period";
        public const string ChallengeDurationPeriod = "Challenge duration";

        private readonly CustomDiscordClient _discordClient;
        private readonly IMatchupService _matchupService;
        private readonly IExceptionHandler _exceptionHandler;
        private readonly IUserService _userService;

        private Matchup _matchup;

        public sealed override DiscordChannel Channel { get; }

        private readonly MatchupController _controller;

        public MatchupContext(Matchup matchup, CustomDiscordClient discordClient, IMatchupService matchupService,
            IExceptionHandler exceptionHandler, IUserService userService) : base(
            (ulong) matchup.DiscordChannelId)
        {
            _matchup = matchup;
            _discordClient = discordClient;
            _matchupService = matchupService;
            _exceptionHandler = exceptionHandler;
            _userService = userService;

            var guild = ResourceLocator.DiscordClient.GetNullsGuild();
            Channel = _discordClient.GetChannel(guild, (ulong) matchup.DiscordChannelId);

            _controller = new MatchupController(matchup, Channel, _matchupService);

            if (Channel == null)
                throw new InvalidOperationException("Discord channel is invalid");

            TimerService.Instance.Register(new TimerService.TimerRegistration
            {
                Interval = TimeSpan.FromHours(1),
                Task = OnTimePass
            }.FireOnNextFullHour());

            ClearChannel();
        }

        private async void OnTimePass()
        {
            try
            {
                await _controller.ProcessTimePass(ResourceLocator.ExecutionContext);
            }
            catch (Exception e)
            {
                _exceptionHandler.Handle(e);
            }
        }


        #region Commands


        [CommandHandler(Regex = @"imupforit",HumanReadableCommand = "imupforit",HelpText = "Adds you to particiapnts of a matchup.")]
        public async Task SignupMe(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            using (var userService = _userService.ObtainLifetimeHandle(executionContext))
            using (var matchupService = _matchupService.ObtainLifetimeHandle(executionContext))
            {
                try
                {
                    var user = await userService.GetOrCreateUser(args.Author);

                    matchupService.ConfigureIncludes().WithChain(query => query.Include(m => m.Participants).ThenInclude(um => um.User)).Commit();
                    _matchup = await matchupService.GetMatchup(_matchup.Id);
                    _controller.Matchup = _matchup;

                    if (_matchup.Participants.Any(participant => participant.User.Equals(user)))
                    {
                        await Channel.SendTimedMessage("You are already participating in this challenge!");
                        return;
                    }

                    _matchup.Participants.Add(new UserMatchup { Matchup = _matchup, User = user });

                    await _controller.UpdateOpeningMessage();
                }
                finally
                {
                    await args.Message.DeleteAsync();
                }       
            }
        }

        [CommandHandler(Regex = @"ormaybenot", HumanReadableCommand = "ormaybenot", HelpText = "Removes you from particiapnts of a matchup.")]
        public async Task RemoveMe(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            using (var userService = _userService.ObtainLifetimeHandle(executionContext))
            using (var matchupService = _matchupService.ObtainLifetimeHandle(executionContext))
            {
                try
                {
                    matchupService.ConfigureIncludes().WithChain(query => query.Include(m => m.Participants)).Commit();
                    _matchup = await matchupService.GetMatchup(_matchup.Id);

                    if(_matchup.CurrentState != Matchup.State.Submissions)
                        return;

                    var user = await userService.GetOrCreateUser(args.Author);


                    _controller.Matchup = _matchup;

                    var userMatchup = _matchup.Participants.FirstOrDefault(participant => participant.User.Equals(user));

                    if (userMatchup == null)
                    {
                        await Channel.SendTimedMessage("You didn't even sign up for this matchup!");
                        return;
                    }

                    _matchup.Participants.Remove(userMatchup);

                    await _controller.UpdateOpeningMessage();
                }
                finally
                {
                    await args.Message.DeleteAsync();
                }       
            }
        }

        [CommandHandler(Regex = @"challenge .*", HumanReadableCommand = "challenge", HelpText = "Adds challenge for your pair.")]
        public async Task Challenge(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            using (var userService = _userService.ObtainLifetimeHandle(executionContext))
            using (var matchupService = _matchupService.ObtainLifetimeHandle(executionContext))
            {
                try
                {
                    matchupService.ConfigureIncludes()
                        .WithChain(query =>
                        {
                            return query.Include(m => m.MatchupPairs)
                                .ThenInclude(p => p.First)
                                .Include(m => m.MatchupPairs)
                                .ThenInclude(p => p.Second);
                        }).Commit();
                    _matchup = await matchupService.GetMatchup(_matchup.Id);
                    _controller.Matchup = _matchup;

                    if (_matchup.CurrentState != Matchup.State.Running)
                        return;

                    var user = await userService.GetOrCreateUser(args.Author);

                    var pair = _matchup.MatchupPairs.FirstOrDefault(participant =>
                        participant.First.Equals(user) || participant.Second.Equals(user));

                    if (pair == null)
                    {
                        await Channel.SendTimedMessage("You didn't even sign up for this matchup!");
                        return;
                    }


                    var pos = args.Message.Content.IndexOf(' ');
                    var message = args.Message.Content.Substring(pos + 1);

                    if (message.Length > 500)
                    {
                        await Channel.SendTimedMessage("Your challenge is far too long!");
                        return;
                    }

                    var isFirst = pair.First.Equals(user);

                    if (isFirst)
                    {
                        pair.SecondParticipantsChallenge = message;
                    }
                    else
                    {
                        pair.FirstParticipantsChallenge = message;
                    }

                    await _controller.UpdatePairMessage(pair);

                }
                finally
                {
                    await args.Message.DeleteAsync();
                }
            }
        }



        [CommandHandler(Regex = @"start", Debug = true)]
        public async Task Start(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            using (var matchupService = _matchupService.ObtainLifetimeHandle(executionContext))
            {
                matchupService.ConfigureIncludes()
                    .WithChain(query =>
                    {
                        return query.Include(m => m.MatchupPairs)
                            .ThenInclude(p => p.First)
                            .Include(m => m.MatchupPairs)
                            .ThenInclude(p => p.Second)
                            .Include(m => m.Participants).ThenInclude(p => p.User);
                    }).Commit();
                _controller.Matchup = await matchupService.GetMatchup(_matchup.Id);
                await _controller.TransitionToRunning();
            }
        }

        [CommandHandler(Regex = @"close", Debug = true)]
        public async Task Close(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            using (var matchupService = _matchupService.ObtainLifetimeHandle(executionContext))
            using (var userService = _userService.ObtainLifetimeHandle(executionContext))
            {
                _controller.Matchup = await matchupService.GetMatchup(_matchup.Id);
                await _controller.TransitionToClosed();
            }

        }

        #endregion

        [CommandHandler(FallbackCommand = true)]
        public async Task FallbackCommand(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            if (!args.Author.IsMe())
                await args.Message.DeleteAsync();
        }

        public async Task<long> OnCreate(Matchup matchup)
        {
            var embed = new DiscordEmbedBuilder();

            embed.Title = "What's going on?";
            embed.Description =
                "This is channel where partipants will be mached in pairs. Idea is that each person gives the other one some challenge like \"watch this show\", \"read this manga\" etc.\n\n" +
                "In order to participate in matchup use command:\n" +
                $"`{CommandHandlerAttribute.CommandStarter}imupforit`\n" +
                $"You can also stop participating:\n" +
                $"`{CommandHandlerAttribute.CommandStarter}ormaybenot`\n\n" +
                $"Once entance period has finished you can assign a challenge to your pair using command:\n" +
                $"`{CommandHandlerAttribute.CommandStarter}challenge <challenge>`\n\n" +
                $"I'm assuming we are well behaved individuals so let's have fun with broadening eachothers' ~~sh^t~~ unripe tastes.";

            embed.Color = DiscordColor.Gray;
            embed.AddField(ParticipantsKey, "-");
            embed.AddField(SubmissionsUntilKey, $"{DateTime.UtcNow} - {matchup.SignupsEndDate}");
            embed.AddField(ChallengeDurationPeriod, $"{(matchup.ChallengesEndDate - matchup.SignupsEndDate).Days} days");

            return (long)(await Channel.SendMessageAsync(null, false, embed.Build())).Id;
        }

    }
}
