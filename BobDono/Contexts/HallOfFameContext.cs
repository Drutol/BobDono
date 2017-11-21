using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BobDono.Core.Attributes;
using BobDono.Core.Extensions;
using BobDono.Interfaces;
using BobDono.Models.Entities.Simple;
using DSharpPlus.EventArgs;

namespace BobDono.Contexts
{
    //[Module(Hidden = true,IsChannelContextual = true)]
    public class HallOfFameContext : ContextModuleBase
    {
        public sealed override ulong? ChannelIdContext { get; protected set; }

        public HallOfFameContext(HallOfFameChannel channel)
        {
            ChannelIdContext = (ulong) channel.DiscordChannelId;
        }

        [CommandHandler(FallbackCommand = true)]
        public async Task FallbackCommand(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            if (!args.Author.IsMe())
                await args.Message.DeleteAsync();
        }
    }
}
