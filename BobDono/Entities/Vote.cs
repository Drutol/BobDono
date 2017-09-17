using System;
using System.Collections.Generic;
using System.Text;

namespace BobDono.Entities
{
    public class Vote
    {
        public long Id { get; set; }

        public Bracket Bracket { get; set; }
        public Waifu VotedWaifu { get; set; }
        public User User { get; set; }
    }
}
