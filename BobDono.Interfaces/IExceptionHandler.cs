using System;
using System.Collections.Generic;
using DSharpPlus.EventArgs;

namespace BobDono.Interfaces
{
    public interface IExceptionHandler
    {
        List<Exception> CaughtThings { get; set; }
        string Handle(Exception e);
        string Handle(Exception e, MessageCreateEventArgs args);
        void Handle(Exception e, string comment);
    }
}