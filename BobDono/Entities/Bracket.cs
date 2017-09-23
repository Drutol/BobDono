using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;

namespace BobDono.Entities
{
    public class Bracket
    {
        public long Id { get; set; }

        public long BracketStageId { get; set; }
        public BracketStage BracketStage { get; set; }

        public WaifuContender FirstWaifu { get; set; }
        public WaifuContender SecondWaifu { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
