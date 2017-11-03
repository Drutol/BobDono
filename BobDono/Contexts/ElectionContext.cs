using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BobDono.Core;
using BobDono.Core.Attributes;
using BobDono.Core.BL;
using BobDono.Core.Controllers;
using BobDono.Core.Extensions;
using BobDono.Core.Utils;
using BobDono.DataAccess.Database;
using BobDono.DataAccess.Services;
using BobDono.Interfaces;
using BobDono.Models.Entities;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace BobDono.Contexts
{
    [Module(IsChannelContextual = true)]
    public class ElectionContext : ContextModuleBase
    {
        private Election _election;
        private DiscordChannel _channel;

        public sealed override ulong? ChannelIdContext { get; protected set; }

        private readonly ElectionController _controller;

        private readonly IWaifuService _waifuService;
        private readonly IElectionService _electionService;
        private readonly IUserService _userService;
        private readonly IContenderService _contenderService;

        public ElectionContext(Election election, IWaifuService waifuService, IElectionService electionService,
            IUserService userService, IContenderService contenderService)
        {
            _waifuService = waifuService;
            _electionService = electionService;
            _userService = userService;
            _contenderService = contenderService;

            _election = election;
            _channel = ResourceLocator.DiscordClient.GetNullsGuild().GetChannel(election.DiscordChannelId);

            if (_channel == null)
                throw new InvalidOperationException("Discord channel is invalid");

            ChannelIdContext = election.DiscordChannelId;

            _controller = new ElectionController(_election, _channel, _electionService);

            TimerService.Instance.Register(
                new TimerService.TimerRegistration
                {
                    Interval = TimeSpan.FromHours(1),
                    Task = OnHourPassed
                }.FireOnNextFullHour());

            ClearChannel();
        }

        #region Commands

        [CommandHandler(Regex = @"add contender \d+\s?(.*)?",
            HumanReadableCommand = "add contender <malId> [imageOverride]",
            HelpText =
                "Adds contender to election if election is in submission period. " +
                "Additionaly default image can be overriden in case of default one being insufficient " +
                "to capture the glory of your proposed character.")]
        public async Task AddContender(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            using (var userService = _userService.ObtainLifetimeHandle<UserService>(executionContext))
            using (var contenderService = _contenderService.ObtainLifetimeHandle<ContenderService>(executionContext))
            using (var electionService = _electionService.ObtainLifetimeHandle<ElectionService>(executionContext))
            using (var waifuService = _waifuService.ObtainLifetimeHandle<WaifuService>(executionContext))
            {
                _election = await electionService.GetElection(_election.Id);
                if (_election.CurrentState == Election.State.Submission)
                {
                    var user = await userService.GetOrCreateUser(args.Author);
                    var count = _election.Contenders?.Count(c => c.Proposer.Id == user.Id);

                    //check if user didn't create more then he should be able to
                    if (count >= _election.EntrantsPerUser)
                    {
                        await args.Channel.SendTimedMessage($"You have already added {count} contestants.");
                        return;
                    }

                    await args.Channel.TriggerTypingAsync();
                    var arguments = args.Message.Content.Split(" ");

                    var waifu = await waifuService.GetOrCreateWaifu(arguments[2]);
                    var contender = contenderService.CreateContender(user, waifu, _election,
                        arguments.Length == 4 ? arguments[3] : null);
                    _election = await electionService.GetElection(_election.Id);


                    await args.Channel.SendMessageAsync(null, false, contender.GetEmbed());

                    _controller.UpdateOpeningMessage();
                }
                await args.Message.DeleteAsync();
            }
        }

        [CommandHandler(Regex = @"vote \d+ \d+", HumanReadableCommand = "vote <bracketNumber> <contestantNumber>")]
        public async Task Vote(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            using (var userService = _userService.ObtainLifetimeHandle<UserService>(executionContext))
            using (var electionService = _electionService.ObtainLifetimeHandle<ElectionService>(executionContext))
            {
                //prepare parameters
                var parameters = args.Message.Content.Split(' ');
                var bracketId = int.Parse(parameters[1]);
                var contenderId = int.Parse(parameters[2]);

                //obtain entities
                _election = await electionService.GetElection(_election.Id);

                if (_election.CurrentState == Election.State.Voting)
                {

                    var user = await userService.GetOrCreateUser(args.Author);
                    var bracket = _election.BracketStages.Last().Brackets.First(b => b.Number == bracketId);

                    //if user has already voted let's return
                    if (bracket.Votes.Any(vote => vote.User.Id == user.Id))
                    {
                        await args.Channel.SendTimedMessage("You have already voted in this bracket.");
                    }
                    else
                    {
                        WaifuContender contender;
                        if (contenderId == 1)
                            contender = bracket.FirstContender;
                        else if (contenderId == 2)
                            contender = bracket.SecondContender;
                        else
                            contender = bracket.ThirdContender;

                        bracket.Votes.Add(new Vote
                        {
                            Bracket = bracket,
                            Contender = contender,
                            CreateDate = DateTime.UtcNow,
                            User = user
                        });

                        await _channel.SendTimedMessage($"Thanks for submitting your vote for {contender.Waifu.Name}");
                    }                  
                }
            }

            await args.Message.DeleteAsync();
        }

        #region Debug

        [CommandHandler(Regex = @"start")]
        public async Task Start(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            using (var electionService = _electionService.ObtainLifetimeHandle<ElectionService>(executionContext))
            {
                _controller.Election = await electionService.GetElection(_election.Id);
                await _controller.TransitionToVoting();
            }
        }

        [CommandHandler(Regex = @"close")]
        public async Task Close(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            using (var electionService = _electionService.ObtainLifetimeHandle<ElectionService>(executionContext))
            {
                _controller.Election = await electionService.GetElection(_election.Id);
                await _controller.CloseCurrentStage();
            }

        }

        [CommandHandler(Regex = @"random")]
        public async Task AddRandomContenders(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            using (var userService = _userService.ObtainLifetimeHandle<UserService>(executionContext))
            using (var contenderService = _contenderService.ObtainLifetimeHandle<ContenderService>(executionContext))
            using (var electionService = _electionService.ObtainLifetimeHandle<ElectionService>(executionContext))
            using (var waifuService = _waifuService.ObtainLifetimeHandle<WaifuService>(executionContext))
            {
                var user = await userService.GetOrCreateUser(args.Author);
                _election = await electionService.GetElection(_election.Id);

                foreach (var id in new[] {"48391", "13701", "20626", "64167", "118763"})
                {

                    var waifu = await waifuService.GetOrCreateWaifu(id);
                    var contender = contenderService.CreateContender(user, waifu, _election);



                    try
                    {
                        await args.Message.DeleteAsync();
                    }
                    catch (Exception e)
                    {

                    }

                    await args.Channel.SendMessageAsync(null, false, contender.GetEmbed());
                    await Task.Delay(100);
                }
            }
        }

        #endregion

        [CommandHandler(FallbackCommand = true)]
        public async Task FallbackCommand(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            if (!args.Author.IsMe())
                await args.Message.DeleteAsync();
        }

        #endregion

        private async void OnHourPassed()
        {
            _election = await _electionService.OneShotAsync(() => _electionService.GetElection(_election.Id));
            _controller.Election = _election;

            _controller.ProcessTimePass();
        }

        private async void ClearChannel()
        {
            var messages = await _channel.GetMessagesAsync();

            foreach (var message in messages)
            {
                if (!message.Author.IsMe())
                    await message.DeleteAsync();
            }
        }

        public void OnCreated()
        {
            _controller.Initialize();
        }


    }
}
