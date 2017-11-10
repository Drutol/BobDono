using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace BobDono.Interfaces
{
    public interface IDatabaseCommandExecutionContext
    {
        DbContext Context { get; }
    }

    public interface ICommandExecutionContext : IDatabaseCommandExecutionContext
    {

    }
}
