using System;
using System.Collections.Generic;
using System.Text;

namespace BobDono.Models.Entities
{
    public class HallOfFameMember
    {
        public long Id { get; set; }

        public string ElectionName { get; set; }
        public User Owner { get; set; }
        public WaifuContender Contender { get; set; }
        public DateTime WinDate { get; set; }

        public string CommandName { get; set; }
        public string ImageUrl { get; set; }

        public long SeparatorMessageId { get; set; }
        public long ContenderMessageId { get; set; }
        public long InfoMessageId { get; set; }
    }
}
