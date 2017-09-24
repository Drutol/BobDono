using System;
using System.Collections.Generic;
using BobDono.Interfaces;

namespace BobDono.Core.BL
{
    public class ExceptionHandler : IExceptionHandler
    {
        public List<Exception> CaughtThings { get; set; } = new List<Exception>(10);

        public string Handle(Exception e)
        {
            CaughtThings.Add(e);
            return
                $"Oh no! My paint has spilled all other the place, but don't let our negative emotions get better of us! Try to make up with `{e.GetType().Name}`!";
        }
    }
}
