using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace BobDono.Models.Entities.JoinEntities
{
    public class UserMatchup
    {
        public long UserId { get; set; }
        public User User { get; set; }

        public long MatchupId { get; set; }
        public Matchup Matchup { get; set; }

        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserMatchup>()
                .HasKey(pair => new { pair.UserId, pair.MatchupId });

            modelBuilder.Entity<UserMatchup>()
                .HasOne(uw => uw.User)
                .WithMany(u => u.MatchupsParticipatingIn)
                .HasForeignKey(uw => uw.UserId);

            modelBuilder.Entity<UserMatchup>()
                .HasOne(uw => uw.Matchup)
                .WithMany(u => u.Participants)
                .HasForeignKey(uw => uw.MatchupId);
        }
    }
}
