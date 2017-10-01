using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BobDono.Interfaces;
using BobDono.Models;
using BobDono.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BobDono.DataAccess.Database
{
    public class BobDatabaseContext : DbContext
    {
        public BobDatabaseContext()
        {
            
        }

        public DbSet<BracketStage> BracketStages { get; set; }
        public DbSet<Bracket> Brackets { get; set; }
        public DbSet<Election> Elections { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Vote> Votes { get; set; }
        public DbSet<Waifu> Waifus { get; set; }
        public DbSet<WaifuContender> WaifuContenders { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=bob.db",builder => builder.MigrationsAssembly("BobDono"));           
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var modelType in GetClassesFromNamespace())
            {
                modelType.GetMethod("OnModelCreating").Invoke(null, new object[] { modelBuilder });
            }

            base.OnModelCreating(modelBuilder);
        }

        private IEnumerable<Type> GetClassesFromNamespace()
        {
            var @interface = typeof(IModelWithRelation);
            string @namespace = "BobDono.Models.Entities";

            return Assembly.GetAssembly(typeof(UserWaifu))
                .GetTypes()
                .Where(t => t.IsClass && t.Namespace == @namespace && @interface.IsAssignableFrom(t));
        }

    }
}
