using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BobDono.Core.Attributes;
using BobDono.Core.Extensions;
using BobDono.Interfaces;
using BobDono.Interfaces.Services;
using BobDono.Models.Entities.Simple;
using DSharpPlus;
using DSharpPlus.EventArgs;

namespace BobDono.Modules
{
    //[Module(Hidden = true)]
    public class HallOfFameModule
    {
        private readonly IHallOfFameChannelService _hallOfFameChannelService;
        private readonly DiscordClient _discordClient;

        public HallOfFameModule(IHallOfFameChannelService hallOfFameChannelService, DiscordClient discordClient)
        {
            _hallOfFameChannelService = hallOfFameChannelService;
            _discordClient = discordClient;
        }

        [CommandHandler(Regex = "hofcreate",Debug = true)]
        public async Task CreateHallOfFame(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            using (var channelService = _hallOfFameChannelService.ObtainLifetimeHandle(executionContext))
            {
                var guild = _discordClient.GetNullsGuild();
                var category = await guild.GetCategoryChannel(DiscordClientExtensions.ChannelCategory.ElectionsMeta);
                var electionChannel = await guild.CreateChannelAsync("HallOfFame", ChannelType.Text,
                    category);

                channelService.Add(new HallOfFameChannel
                {
                    DiscordChannelId = (long) electionChannel.Id,
                });
            }
        }
    }
}
