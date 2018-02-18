using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BobDono.Interfaces;
using BobDono.Models;
using BobDono.Models.Entities;
using BobDono.Models.Entities.Simple;
using BobDono.Models.Entities.Stats;
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
        public DbSet<TrueWaifu> TrueWaifus { get; set; }
        public DbSet<ExceptionReport> ExceptionReports { get; set; }
        public DbSet<ElectionTheme> ElectionThemes { get; set; }
        public DbSet<HallOfFameMember> HallOfFameMembers { get; set; }
        public DbSet<MerchandiseItem> MerchandiseItems { get; set; }

        //stats
        public DbSet<ExecutedCommand> ExecutedCommands { get; set; }

        //channels
        public DbSet<HallOfFameChannel> HallOfFameChannels { get; set; }
        public DbSet<ElectionThemeChannel> ElectionThemeChannels { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(
                Secrets.ConncetionString, builder => builder.MigrationsAssembly("BobDono"));
            optionsBuilder.EnableSensitiveDataLogging();
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
            var namespaces = new[]
            {
                "BobDono.Models.Entities", "BobDono.Models.MalHell", "BobDono.Models.Entities.JoinEntities", "BobDono.Models.Entities.Stats"
            };

            return Assembly.GetAssembly(typeof(UserWaifu))
                .GetTypes()
                .Where(t => t.IsClass && namespaces.Any(n => n.Equals(t.Namespace)) && @interface.IsAssignableFrom(t));
        }

    }
}
