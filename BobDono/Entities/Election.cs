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
        public long Id { get; set; }

        public ulong DiscordChannelId { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

        public DateTime SubmissionsStartDate { get; set; }
        public DateTime SubmissionsEndDate { get; set; }

        public DateTime VotingStartDate { get; set; }
        public DateTime VotingEndDate { get; set; }

        public virtual ICollection<BracketStage> BracketStages { get; set; }

        public User Author { get; set; }

        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Election>()
                .HasMany(e => e.BracketStages)
                .WithOne(s => s.Election);

            modelBuilder.Entity<Election>()
                .HasOne(e => e.Author)
                .WithMany(a => a.Elections);
        }
    }
}
