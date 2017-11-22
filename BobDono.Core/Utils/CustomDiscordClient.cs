using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DSharpPlus;
using DSharpPlus.Entities;

namespace BobDono.Core.Utils
{
    public class CustomDiscordClient : DiscordClient
    {
        public List<DiscordChannel> CreatedChannels { get; } = new List<DiscordChannel>();

        public CustomDiscordClient(DiscordConfiguration config) : base(config)
        {

        }

        public DiscordChannel GetChannel(DiscordGuild guild, ulong id)
        {
            return CreatedChannels.FirstOrDefault(channel => channel.Id == id) ?? guild.GetChannel(id);
        }
    }
}
