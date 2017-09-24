using System;
using BobDono.Core.Utils;

namespace BobDono.Core.Attributes
{
    public class CommandHandlerAttribute : Attribute
    {
        public const string CommandStarter = "!";

        private string _regex;
        private string _humanReadableCommand;

        public string Regex
        {
            get => _regex;
            set => _regex = $"^{CommandStarter}{value}$";
        }

        public string HumanReadableCommand
        {
            get => _humanReadableCommand;
            set => _humanReadableCommand = CommandStarter + value;
        }

        public bool Authorize { get; set; }
        public ulong? LimitToChannel { get; set; }
        public bool Awaitable { get; set; } = true;
        public string HelpText { get; set; } = "This command doesn't need explanation just like a good painting.";

        public ModuleAttribute ParentModuleAttribute { get; set; }
    }
}
