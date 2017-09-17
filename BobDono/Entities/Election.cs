using System;
using System.Collections.Generic;
using System.Text;

namespace BobDono.Entities
{
    public class Election
    {
        public long Id { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public ICollection<WaifuContender> Contenders { get; set; }
        public ICollection<Bracket> Brackets { get; set; }

        public User Author { get; set; }
    }
}
