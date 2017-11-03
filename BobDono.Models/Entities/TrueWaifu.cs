using System;
using System.Collections.Generic;
using System.Text;

namespace BobDono.Models.Entities
{
    public class TrueWaifu
    {
        public long Id { get; set; }

        public User User { get; set; }
        public Waifu Waifu { get; set; }
        public string Description { get; set; }
        public string FeatureImage { get; set; }
    }
}
