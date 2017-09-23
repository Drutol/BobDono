using System;
using System.Collections.Generic;
using System.Text;
using BobDono.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace BobDono.Entities
{
    public class Election : IModelWithRelation
    {
        private ICollection<WaifuContender> _contenders;
        private ICollection<BracketStage> _bracketStages;
        public long Id { get; set; } = int.MinValue + 1000;

        public ulong DiscordChannelId { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public int EntrantsPerUser { get; set; }

        public DateTime SubmissionsStartDate { get; set; }
        public DateTime SubmissionsEndDate { get; set; }

        public DateTime VotingStartDate { get; set; }
        public DateTime VotingEndDate { get; set; }

        public virtual ICollection<BracketStage> BracketStages =>
            _bracketStages ?? (_bracketStages = new HashSet<BracketStage>());


        public virtual ICollection<WaifuContender> Contenders { get; set; }

        public User Author { get; set; }

        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Election>()
                .HasKey(election => election.Id);

            modelBuilder.Entity<Election>()
                .HasMany(e => e.BracketStages)
                .WithOne(s => s.Election);

            modelBuilder.Entity<Election>()
                .HasOne(e => e.Author)
                .WithMany(a => a.Elections);

            modelBuilder.Entity<Election>()
                .HasMany(e => e.Contenders)
                .WithOne(wc => wc.Election);

        }
    }
}
