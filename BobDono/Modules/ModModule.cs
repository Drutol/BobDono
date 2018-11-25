using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BobDono.Core.Attributes;
using BobDono.Core.Extensions;
using BobDono.Core.Utils;
using BobDono.Interfaces;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace BobDono.Modules
{
    [Module(Hidden = true)]
    public class ModModule
    {
        private readonly CustomDiscordClient _customDiscordClient;

        public ModModule(CustomDiscordClient customDiscordClient)
        {
            _customDiscordClient = customDiscordClient;

            _customDiscordClient.MessageCreated+= HandleSuggestionMessages;
        }

        private async Task HandleSuggestionMessages(MessageCreateEventArgs e)
        {
            if (e.Channel.Id == 505439920580591626)
            {
                await e.Message.CreateReactionAsync(DiscordEmoji.FromName(_customDiscordClient, ":annak:"));
                await e.Message.CreateReactionAsync(DiscordEmoji.FromName(_customDiscordClient, ":rika:"));
            }
        }
    }
}
