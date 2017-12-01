using System;
using System.Collections.Generic;
using System.Text;
using BobDono.Models.Entities.JoinEntities;
using Microsoft.EntityFrameworkCore;

namespace BobDono.Models.Entities
{
    public class Matchup : IModelWithRelation
    {
        public enum State
        {
            Submissions,
            Running,
            Finished,
            ClosedForcibly
        }

        public long Id { get; set; }
        public long DiscordChannelId { get; set; }
        public long OpeningMessageId { get; set; }

        public User Author { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

        public virtual ICollection<UserMatchup> Participants { get; set; } = new List<UserMatchup>();
        public virtual ICollection<MatchupPair> MatchupPairs { get; set; } = new List<MatchupPair>();

        public DateTime SignupsEndDate { get; set; }
        public DateTime ChallengesEndDate { get; set; }

        public State CurrentState { get; set; }

        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Matchup>()
                .HasMany(m => m.MatchupPairs)
                .WithOne(pair => pair.Matchup);
        }
    }
}
