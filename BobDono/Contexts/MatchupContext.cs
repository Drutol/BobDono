using System;
using System.Collections.Generic;
using System.Text;
using BobDono.Core.Attributes;

namespace BobDono.Contexts
{
    //[Module(IsChannelContextual = true)]
    public class MatchupContext : ContextModuleBase
    {
        public override ulong? ChannelIdContext { get; protected set; }


        public MatchupContext()
        {
            
        }
    }
}
