using System;
using System.Collections.Generic;
using System.Text;
using BobDono.DataAccess.Database;
using BobDono.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BobDono.DataAccess
{
    public class CommandExecutionContext : ICommandExecutionContext
    {
        public DbContext Context { get; } = new BobDatabaseContext();

        public bool AuthenticatedCaller { get; set; }
    }
}
