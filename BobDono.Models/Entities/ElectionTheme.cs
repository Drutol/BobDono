using System;
using System.Collections.Generic;
using System.Text;

namespace BobDono.Models.Entities
{
    public class ElectionTheme 
    {
        public long Id { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }

        public User Proposer { get; set; }
        public int Approvals { get; set; }

        public long DiscordMessageId { get; set; }
        public bool Used { get; set; }
    }
}
