using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BobDono.Attributes;
using BobDono.Entities;
using BobDono.Interfaces;
using BobDono.Utils;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

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

        [CommandHandler(Regex = @"add contender \d+\s?(.*)?",
            HumanReadableCommand = "add contender <malId> [imageOverride]",
            HelpText =
                "Adds contender to election if election is in submission period. " +
                "Additionaly default image can be overriden in case of default one being insufficient " +
                "to capture the glory of your proposed character.")]
        public async Task AddContender(MessageCreateEventArgs args)
        {

        }


    }
}
