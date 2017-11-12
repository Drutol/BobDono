using System;
using System.Collections.Generic;
using System.Text;

namespace BobDono.Models.Entities
{
    public class Matchup
    {
        public long Id { get; set; }

        public User Author { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

        public virtual ICollection<User> Participants { get; set; } = new List<User>();
        public virtual ICollection<MatchupPair> MatchupPairs { get; set; } = new List<MatchupPair>();

        public DateTime SignupsEndDate { get; set; }
        public DateTime ChallengesEndDate { get; set; }
    }
}
