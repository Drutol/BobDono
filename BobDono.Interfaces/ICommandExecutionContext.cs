using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace BobDono.Interfaces
{
    public interface ICommandExecutionContext
    {
        DbContext Context { get; }
    }
}
