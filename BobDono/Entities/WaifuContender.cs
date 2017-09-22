using System;
using System.Collections.Generic;
using System.Text;

namespace BobDono.Entities
{
    public class WaifuContender
    {
        public long Id { get; set; }
        public int SeedNumber { get; set; }

        public User Proposer { get; set; }
        public Waifu Waifu { get; set; }

        public ICollection<Vote> Votes { get; set; }
    }
}
