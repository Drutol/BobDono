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

        private ElectionController _controller;

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
        public async Task AddContender(MessageCreateEventArgs args)
        {
            var user = await _userService.GetOrCreateUser(args.Author);

            _election = await _electionService.GetElection(_election.Id);

            var count = _election.Contenders?.Count(c => c.Proposer.Id == user.Id);

            //check if user didn't create more then he should be able to
            if (count >= _election.EntrantsPerUser)
            {
                await args.Channel.SendMessageAsync($"You have already added {count} contestants.");
                return;
            }

            await args.Channel.TriggerTypingAsync();

            var arguments = args.Message.Content.Split(" ");

            var waifu = await _waifuService.GetOrCreateWaifu(arguments[2]);

            var contender = await _contenderService.CreateContender(user, waifu, _election,
                arguments.Length == 4 ? arguments[3] : null);

            _election = await _electionService.GetElection(_election.Id);

            await args.Message.DeleteAsync();

            await args.Channel.SendMessageAsync(null, false, contender.GetEmbed());

            _controller.UpdateOpeningMessage();
        }

        [CommandHandler(Regex = @"start")]
        public async Task Start(MessageCreateEventArgs args)
        {
            await _controller.TransitionToVoting();
        }

        [CommandHandler(Regex = @"random")]
        public async Task AddRandomContenders(MessageCreateEventArgs args)
        {
            var user = await _userService.GetOrCreateUser(args.Author);

            foreach (var id in new[] {"48391","13701","20626","64167","118763"})
            {
                var waifu = await _waifuService.GetOrCreateWaifu(id);

                var contender = await _contenderService.CreateContender(user, waifu, _election);

                _election = await _electionService.GetElection(_election.Id);

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

        [CommandHandler(FallbackCommand = true)]
        public async Task FallbackCommand(MessageCreateEventArgs args)
        {
            if (!args.Author.IsMe())
                await args.Message.DeleteAsync();
        }

        #endregion

        private async void OnHourPassed()
        {
            _election = await _electionService.GetElection(_election.Id);
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
