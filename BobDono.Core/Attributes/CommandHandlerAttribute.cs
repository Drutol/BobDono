using System;
using System.Runtime.CompilerServices;
using BobDono.Core.Utils;

namespace BobDono.Core.Attributes
{
    public class CommandHandlerAttribute : Attribute
    {
#if DEBUG
        public const string CommandStarter = "b/";
#else
        public const string CommandStarter = "b/";
#endif

        public string HandlerMethodName { get; }

        public CommandHandlerAttribute([CallerMemberName] string handlerMethodName = null)
        {
            HandlerMethodName = handlerMethodName;
        }

        private string _regex;
        private string _humanReadableCommand;

        public string Regex
        {
            get => _regex;
            set => _regex = IgnoreRegexWrap? value : $"^{CommandStarter}{value}$";
        }

        public string HumanReadableCommand
        {
            get => _humanReadableCommand;
            set => _humanReadableCommand = IgnoreRegexWrap ? value : CommandStarter + value;
        }

        public bool Authorize { get; set; }
        public ulong? LimitToChannel { get; set; }
        public bool Awaitable { get; set; } = true;
        public string HelpText { get; set; } /*= "This command doesn't need explanation just like a good painting.";*/
        public bool FallbackCommand { get; set; }
        public bool Debug { get; set; }
        public bool IgnoreRegexWrap { get; set; }
        public bool Hidden { get; set; }
        public bool AcceptsBotCalls { get; set; }
        public bool AllowInContextChannels { get; set; }

        public ModuleAttribute ParentModuleAttribute { get; set; }

    }
}
