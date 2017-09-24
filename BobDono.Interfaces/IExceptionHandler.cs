using System;
using System.Collections.Generic;

namespace BobDono.Interfaces
{
    public interface IExceptionHandler
    {
        List<Exception> CaughtThings { get; set; }
        string Handle(Exception e);
    }
}