using System;
using System.Collections.Generic;
using System.Text;
using BobDono.DataAccess.Database;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace BobDono
{
    public class DatabaseContextFactory : IDesignTimeDbContextFactory<BobDatabaseContext>
    {
        public BobDatabaseContext CreateDbContext(string[] args)
        {
            return new BobDatabaseContext();
        }
    }
}
