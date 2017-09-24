using System;

namespace BobDono.Models.Entities
{
    public class Bracket
    {
        public long Id { get; set; }
        public BracketStage BracketStage { get; set; }

        public WaifuContender FirstWaifu { get; set; }
        public WaifuContender SecondWaifu { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
