using System;
using System.Collections.Generic;
using System.Text;

namespace BobDono.Models.Entities
{
    public class QuizAnswer
    {
        public long Id { get; set; }

        public QuizSession Session { get; set; }
        public QuizQuestion Question { get; set; }

        public string Answer { get; set; }
        public bool IsCorrect { get; set; }
    }
}
