﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BobDono.Models.Entities.Simple
{
    public class ElectionThemeChannel
    {
        public long Id { get; set; }

        public long DiscordChannelId { get; set; }
        public long OpeningMessageId { get; set; }

        public DateTime NextElection { get; set; }
    }
}
