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
        private DiscordChannel _channel;

        public sealed override ulong? ChannelIdContext { get; protected set; }


        private readonly IWaifuService _waifuService;
        private readonly IElectionService _electionService;
        private readonly IUserService _userService;
        private readonly IContenderService _contenderService;

        public ElectionContext(Election election, IWaifuService waifuService, IElectionService electionService, IUserService userService, IContenderService contenderService)
        {
            _waifuService = waifuService;
            _electionService = electionService;
            _userService = userService;
            _contenderService = contenderService;

            _election = election;
            _channel = ResourceLocator.DiscordClient.GetNullsGuild().GetChannel(election.DiscordChannelId);

            if(_channel == null)
                throw new InvalidOperationException("Discord channel is invalid");

            ChannelIdContext = election.DiscordChannelId;

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

        [CommandHandler(FallbackCommand = true)]
        public async Task FallbackCommand(MessageCreateEventArgs args)
        {
            await args.Message.DeleteAsync();
        }

        #endregion

        private void OnHourPassed()
        {

        }

        private async void ClearChannel()
        {
            var messages = await _channel.GetMessagesAsync();

            foreach (var message in messages)
            {
                if (!message.Author.IsBot)
                    await message.DeleteAsync();
            }
        }

        public void OnCreated()
        {
            var embed = new DiscordEmbedBuilder();

            embed.Color = DiscordColor.Gold;
            embed.Description = _election.Description;
            embed.Title = $"Election: {_election.Name}";
            embed.Author = new DiscordEmbedBuilder.EmbedAuthor {Name = _election.Author.Name};
            embed.AddField("Submission time:",
                $"{_election.SubmissionsStartDate} - {_election.SubmissionsEndDate} - *({(_election.SubmissionsEndDate - _election.SubmissionsStartDate).Days} days)*");
            embed.AddField("Entrants per person:", _election.EntrantsPerUser.ToString());
        }

    }
}
