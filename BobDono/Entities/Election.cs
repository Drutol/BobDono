using System;
using System.Collections.Generic;
using System.Text;

namespace BobDono.Entities
{
    public class Election
    {
        public long Id { get; set; }
        public ulong DiscordChannelId { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

        public DateTime SubmissionsStartDate { get; set; }
        public DateTime SubmissionsEndDate { get; set; }

        public DateTime VotingStartDate { get; set; }
        public DateTime VotingEndDate { get; set; }

        public ICollection<BracketStage> BracketStages { get; set; }

        public User Author { get; set; }
    }
}
