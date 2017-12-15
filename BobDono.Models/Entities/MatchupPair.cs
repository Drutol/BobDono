using System;
using System.Collections.Generic;
using System.Text;

namespace BobDono.Models.Entities
{
    public class MatchupPair
    {
        public long Id { get; set; }
        public long DiscordMessageId { get; set; }
        public int Number { get; set; }

        public Matchup Matchup { get; set; }

        public User First { get; set; }
        public User Second { get; set; }

        public string FirstParticipantsChallenge { get; set; }
        public string SecondParticipantsChallenge { get; set; }

        public DateTime FirstParticipantsChallengeCompletionDate { get; set; }
        public DateTime SecondParticipantsChallengeCompletionDate { get; set; }

        public string FirstNotes { get; set; }
        public string SecondNotes { get; set; }

    }
}
