using System.Collections.Generic;
using System.Threading.Tasks;
using BobDono.Core.Attributes;
using BobDono.Core.BL;
using BobDono.Core.Interfaces;
using BobDono.MalHell.Comm;
using BobDono.MalHell.Queries;
using DSharpPlus;

namespace BobDono.Core.Utils
{
    public class BotContext : IBotContext
    {
        private readonly DiscordClient _client;

        public BotContext(DiscordClient client)
        {
            _client = client;

            _client.MessageCreated += args =>
            {
                if (args.Channel.IsPrivate)
                    NewPrivateMessage?.Invoke(args);
                return Task.CompletedTask;
            };
        }

        public Dictionary<ModuleAttribute, List<CommandHandlerAttribute>> Commands { get; set; }
        public event Delegates.CommandHandlerDelegate NewPrivateMessage;   
    }
}
