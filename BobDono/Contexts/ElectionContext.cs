using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BobDono.Attributes;
using BobDono.BL;
using BobDono.BL.Services;
using BobDono.Entities;
using BobDono.Extensions;
using BobDono.Interfaces;
using BobDono.Utils;
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

        public ElectionContext(Election election)
        {
            _election = election;
            _channel = BotContext.DiscordClient.GetNullsGuild().GetChannel(election.DiscordChannelId);
            ChannelIdContext = election.DiscordChannelId;

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
            var user = await UserService.Instance.GetOrCreateUser(args.Author);

            _election = await ElectionService.Instance.GetElection(_election.Id);

            var count = _election.Contenders?.Count(c => c.Proposer.Id == user.Id);

            //check if user didn't create more then he should be able to
            if (count > _election.EntrantsPerUser)
            {
                await args.Channel.SendMessageAsync($"You have already added {count} contestants.");
                return;
            }

            await args.Channel.TriggerTypingAsync();

            var arguments = args.Message.Content.Split(" ");

            var waifu = await WaifuService.Instance.GetOrCreateWaifu(arguments[2]);

            var contender = await ContenderService.Instance.CreateContender(user, waifu, _election,
                arguments.Length == 4 ? arguments[3] : null);

            _election = await ElectionService.Instance.GetElection(_election.Id);

            await args.Message.DeleteAsync();

            await args.Channel.SendMessageAsync(null, false, contender.GetEmbed());
        }



        private void OnHourPassed()
        {

        }

    }
}
