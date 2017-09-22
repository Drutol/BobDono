using System;
using System.Collections.Generic;
using System.Text;

namespace BobDono.Entities
{
    public class BracketStage
    {
        public long Id { get; set; }

        public ICollection<Bracket> Brackets { get; set; }
    }
}
