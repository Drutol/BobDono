using System;
using System.Collections.Generic;
using System.Text;

namespace BobDono.Models.Entities
{
    public class QuizQuestion
    {
        public long Id { get; set; }

        public string Question { get; set; }
        public string[] Answers { get; set; } = new string[0];
        public int Points { get; set; }
        public string Author { get; set; }
        public string Hint { get; set; }

        public int QuestionBatch { get; set; }
        public DateTime CreatedDate { get; set; }

        public string ReactionSuccess { get; set; }
        public string ReactionFailure { get; set; }

        public QuizSession.QuizQuestionSet Set { get; set; }
    }
}
