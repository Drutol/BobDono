using System;
using System.Collections.Generic;
using System.Text;
using BobDono.Core.Attributes;
using BobDono.Models.Entities;
using DSharpPlus.Entities;

namespace BobDono.Contexts
{
    //[Module(IsChannelContextual = true)]
    public class MatchupContext : ContextModuleBase
    {
        public override DiscordChannel Channel { get; }


        public MatchupContext(Matchup matchup) : base((ulong)matchup.DiscordChannelId)
        {
            
        }

        public void OnCreated()
        {
            
        }
    }
}
