using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BobDono.Interfaces;
using DSharpPlus.EventArgs;

namespace BobDono.Utils
{
    public static class Delegates
    {
        public delegate Task CommandHandlerDelegateAsync(MessageCreateEventArgs args);
        public delegate void CommandHandlerDelegate(MessageCreateEventArgs args);
        public delegate Task ContextualCommandHandlerDelegateAsync(MessageCreateEventArgs args, IModule context);
    }
}
