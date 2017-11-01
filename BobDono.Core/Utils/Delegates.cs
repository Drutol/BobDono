using System.Threading.Tasks;
using BobDono.Core.Interfaces;
using BobDono.Interfaces;
using DSharpPlus.EventArgs;

namespace BobDono.Core.Utils
{
    public static class Delegates
    {
        public delegate void MessageDelegate(MessageCreateEventArgs args);
        public delegate Task CommandHandlerDelegateAsync(MessageCreateEventArgs args, ICommandExecutionContext executionContext);
        public delegate void CommandHandlerDelegate(MessageCreateEventArgs args, ICommandExecutionContext executionContext);
        public delegate Task ContextualCommandHandlerDelegateAsync(MessageCreateEventArgs args, IModule context, ICommandExecutionContext executionContext);
    }
}
