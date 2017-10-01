using System;
using System.Collections.Generic;
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
        }

        private ICollection<WaifuContender> _contenders;
        private ICollection<BracketStage> _bracketStages;
        public long Id { get; set; }

        public ulong DiscordChannelId { get; set; }

        public ulong OpeningMessageId { get; set; }
        public ulong PendingVotingStartMessageId { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public int EntrantsPerUser { get; set; }

        public DateTime SubmissionsStartDate { get; set; }
        public DateTime SubmissionsEndDate { get; set; }

        public DateTime VotingStartDate { get; set; }
        public DateTime VotingEndDate { get; set; }

        public User Author { get; set; }

        public State CurrentState { get; set; } = State.Submission;

        public virtual ICollection<BracketStage> BracketStages =>
            _bracketStages ?? (_bracketStages = new HashSet<BracketStage>());

        public virtual ICollection<WaifuContender> Contenders =>
            _contenders ?? (_contenders = new HashSet<WaifuContender>());

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
