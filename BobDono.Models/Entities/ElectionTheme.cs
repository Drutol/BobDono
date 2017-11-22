using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace BobDono.Models.Entities
{
    public class ElectionTheme
    {
        public long Id { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }

        public long DiscordMessageId { get; set; }
        public bool Used { get; set; }

        public User Proposer { get; set; }
        public virtual ICollection<UserTheme> Approvals { get; set; } = new HashSet<UserTheme>();
        public bool Approved { get; set; }

        public DateTime CreateDate { get; set; }
        public DateTime ElectionCreateDate { get; set; }
    }
}
