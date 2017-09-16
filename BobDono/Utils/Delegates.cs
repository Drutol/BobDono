using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.EventArgs;

namespace BobDono.Utils
{
    public static class Delegates
    {
        public delegate Task CommandHandlerDelegate(MessageCreateEventArgs args);
    }
}
