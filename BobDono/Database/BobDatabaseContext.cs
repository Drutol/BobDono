using System;
using System.Collections.Generic;
using System.Text;
using BobDono.Entities;
using Microsoft.EntityFrameworkCore;

namespace BobDono.Database
{
    public class BobDatabaseContext : DbContext
    {
        public DbSet<Bracket> Brackets { get; set; }
        public DbSet<Election> Elections { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Vote> Votes { get; set; }
        public DbSet<Waifu> Waifus { get; set; }
        public DbSet<WaifuContender> WaifuContenders { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=bob.db");
        }
    }
}
