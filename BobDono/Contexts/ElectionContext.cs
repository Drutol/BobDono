using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using BobDono.Attributes;
using BobDono.Entities;
using BobDono.Interfaces;
using BobDono.Utils;
using DSharpPlus.Entities;

namespace BobDono.Contexts
{
    [Module(IsChannelContextual = true)]
    public class ElectionContext : ContextModuleBase
    {
        private readonly Election _election;
        private readonly DiscordChannel _channel;

        public override ulong? ChannelIdContext { get; }

        public ElectionContext(Election election) 
        {
            _election = election;
            _channel = BotContext.DiscordClient.GetNullsGuild().GetChannel(election.DiscordChannelId);
            ChannelIdContext = election.DiscordChannelId;
        }


    }
}
