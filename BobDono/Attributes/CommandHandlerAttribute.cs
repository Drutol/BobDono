using System;
using System.Collections.Generic;
using System.Text;
using BobDono.Utils;

namespace BobDono.Attributes
{
    public class CommandHandlerAttribute : Attribute
    {
        private string _regex;
        private string _humanReadableCommand;

        public string Regex
        {
            get => _regex;
            set => _regex = $"^{BotContext.CommandStarter}{value}$";
        }

        public string HumanReadableCommand
        {
            get => _humanReadableCommand;
            set => _humanReadableCommand = BotContext.CommandStarter + value;
        }

        public bool Authorize { get; set; }
        public ulong? LimitToChannel { get; set; }
        public bool Awaitable { get; set; } = true;
        public string HelpText { get; set; } = "This command doesn't need explanation just like a good painting.";

        public ModuleAttribute ParentModuleAttribute { get; set; }
    }
}
