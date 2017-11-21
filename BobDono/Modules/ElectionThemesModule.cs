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
    public class ElectionThemesModule
    {
        private readonly IElectionThemesChannelService _electionThemesChannelService;
        private readonly DiscordClient _discordClient;

        public ElectionThemesModule(IElectionThemesChannelService electionThemesChannelService, DiscordClient discordClient)
        {
            _electionThemesChannelService = electionThemesChannelService;
            _discordClient = discordClient;
        }

        [CommandHandler(Regex = "ethemescreate", Debug = true)]
        public async Task CreateElectionThemesChannel(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            using (var channelService = _electionThemesChannelService.ObtainLifetimeHandle(executionContext))
            {
                var guild = _discordClient.GetNullsGuild();
                var category = await guild.GetCategoryChannel(DiscordClientExtensions.ChannelCategory.ElectionsMeta);
                var electionChannel = await guild.CreateChannelAsync("ElectionThemes", ChannelType.Text,
                    category);

                channelService.Add(new ElectionThemeChannel
                {
                    DiscordChannelId = (long)electionChannel.Id,
                });
            }
        }
    }
}
