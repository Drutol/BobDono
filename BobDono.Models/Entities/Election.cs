using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace BobDono.Models.Entities
{
    public class Election : IModelWithRelation
    {
        public enum State
        {
            Submission,
            PedningVotingStart,
            Voting,
            Closed,
            ClosedForcibly
        }


        public long Id { get; set; }

        #region MessageIds
        public long _discordChannelId { get; set; }
        public long _openingMessageId { get; set; }
        public long _pendingVotingStartMessageId { get; set; }
        public long _resultsMessageId { get; set; }

        [NotMapped]
        public ulong DiscordChannelId
        {
            get { return (ulong)_discordChannelId; }
            set { _discordChannelId = (long)value; }
        }

        [NotMapped]
        public ulong OpeningMessageId
        {
            get { return (ulong)_openingMessageId; }
            set { _openingMessageId = (long)value; }
        }

        [NotMapped]
        public ulong PendingVotingStartMessageId
        {
            get { return (ulong)_pendingVotingStartMessageId; }
            set { _pendingVotingStartMessageId = (long)value; }
        }

        [NotMapped]
        public ulong ResultsMessageId
        {
            get { return (ulong)_resultsMessageId; }
            set { _resultsMessageId = (long)value; }
        }

        public string BracketMessagesIdsBlob { get; set; }

        [NotMapped]
        public List<ulong> BracketMessagesIds
        {
            get { return BracketMessagesIdsBlob?.Split(';').Select(ulong.Parse).ToList() ?? new List<ulong>(); }
            set { BracketMessagesIdsBlob = string.Join(";", value.Select(u => u.ToString())); }
        }
        #endregion


        public string Name { get; set; }
        public string Description { get; set; }
        public int EntrantsPerUser { get; set; }
        public bool FeatureImageRequired { get; set; }

        public DateTime SubmissionsStartDate { get; set; }
        public DateTime SubmissionsEndDate { get; set; }

        public DateTime VotingStartDate { get; set; }
        public DateTime VotingEndDate { get; set; }
        public int StageCount { get; set; }
        
        public User Author { get; set; }

        public State CurrentState { get; set; } = State.Submission;

        public virtual ICollection<BracketStage> BracketStages { get; set; } = new HashSet<BracketStage>();
        public virtual ICollection<WaifuContender> Contenders { get; set; } = new List<WaifuContender>();

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

        private sealed class IdEqualityComparer : IEqualityComparer<Election>
        {
            public bool Equals(Election x, Election y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.Id == y.Id;
            }

            public int GetHashCode(Election obj)
            {
                return obj.Id.GetHashCode();
            }
        }

        [NotMapped]
        public static IEqualityComparer<Election> IdComparer { get; } = new IdEqualityComparer();
    }
}
