using System;
using System.Collections.Generic;
using System.Text;

namespace BobDono.Entities
{
    public class Waifu
    {
        public long Id { get; set; }

        public string Name { get; set; }
        public string MalId { get; set; }
        public string ImageUrl { get; set; }
        public string Description { get; set; }
    }
}
