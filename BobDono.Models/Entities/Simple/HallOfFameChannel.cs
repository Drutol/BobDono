using System;
using System.Collections.Generic;
using System.Text;

namespace BobDono.Models.Entities.Simple
{
    public class HallOfFameChannel
    {
        public long Id { get; set; }

        public long DiscordChannelId { get; set; }
        public long OpeningMessageId { get; set; }
    }
}
