using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BobDono.Core;
using BobDono.Core.Attributes;
using BobDono.Core.BL;
using BobDono.Core.Extensions;
using BobDono.Core.Utils;
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
        private readonly DiscordChannel _channel;

        public override ulong? ChannelIdContext { get; }


        private readonly IWaifuService _waifuService;
        private readonly IElectionService _electionService;
        private readonly IUserService _userService;
        private readonly IContenderService _contenderService;

        public ElectionContext(Election election)
        {
            _election = election;
            _channel = ResourceLocator.DiscordClient.GetNullsGuild().GetChannel(election.DiscordChannelId);
            ChannelIdContext = election.DiscordChannelId;

            _waifuService = ResourceLocator.WaifuService;
            _electionService = ResourceLocator.ElectionService;
            _userService = ResourceLocator.UserService;
            _contenderService = ResourceLocator.ContenderService;

            TimerService.Instance.Register(
                new TimerService.TimerRegistration
                    {
                        Interval = TimeSpan.FromHours(1),
                        Task = OnHourPassed
                    }.FireOnNextFullHour());
        }


        #region Commands

        

        #endregion

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
            if (count > _election.EntrantsPerUser)
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
        }



        private void OnHourPassed()
        {

        }

    }
}
