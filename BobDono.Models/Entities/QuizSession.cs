using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace BobDono.Models.Entities
{
    public class QuizSession : IModelWithRelation
    {

        public enum QuizQuestionSet
        {
            Trivia
        }

        public enum QuizStatus
        {
            InProgress,
            Finished,
            TimedOut,
            Errored
        }

        public long Id { get; set; }

        public User User { get; set; }
        public ICollection<QuizAnswer> Answers { get; set; } = new List<QuizAnswer>();
        public int QuestionsCount { get; set; }

        public DateTime Started { get; set; }
        public DateTime Finished { get; set; }

        public int RemainingChances { get; set; }
        public int TotalChances { get; set; }
        public int Score { get; set; }
        public int AdditonalScoreFromPreviousBatches { get; set; }

        public int? CompletedBatch { get; set; }
        public int SessionBatch { get; set; }

        public QuizQuestionSet Set { get; set; }
        public QuizStatus Status { get; set; }


        //WORKING COPY
        [NotMapped]
        public List<QuizQuestion> QuestionWorkSet { get; set; }

    
        public static void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<QuizSession>()
                .HasMany(s => s.Answers)
                .WithOne(a => a.Session);

            modelBuilder.Entity<QuizSession>()
                .HasOne(s => s.User)
                .WithMany(a => a.QuizSessions);
        }

        protected bool Equals(QuizSession other)
        {
            return Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((QuizSession)obj);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

    }
}
