using System;
using System.Collections.Generic;
using System.Text;
using BobDono.Database;
using Microsoft.EntityFrameworkCore;

namespace BobDono.Entities
{
    public class BracketStage : IModelWithRelation
    {
        public long Id { get; set; }
        public Election Election { get; set; }
        public ICollection<Bracket> Brackets { get; set; }

        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BracketStage>()
                .HasOne(bs => bs.Election)
                .WithMany(election => election.BracketStages);

            modelBuilder.Entity<BracketStage>()
                .HasMany(bs => bs.Brackets)
                .WithOne(bracket => bracket.BracketStage);
        }
    }
}
