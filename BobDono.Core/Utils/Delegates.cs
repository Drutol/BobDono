using System.Threading.Tasks;
using BobDono.Core.Interfaces;
using BobDono.Interfaces;
using DSharpPlus.EventArgs;

namespace BobDono.Core.Utils
{
    public static class Delegates
    {
        public delegate Task CommandHandlerDelegateAsync(MessageCreateEventArgs args);
        public delegate void CommandHandlerDelegate(MessageCreateEventArgs args);
        public delegate Task ContextualCommandHandlerDelegateAsync(MessageCreateEventArgs args, IModule context);
    }
}
