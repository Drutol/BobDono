using System;
using System.Collections.Generic;
using System.Text;
using BobDono.Core.Attributes;
using BobDono.Interfaces.Services;

namespace BobDono.Modules
{
    [Module(Name = "Matchups",Description = "Module for creating and managing matchups.")]
    public class MatchupModule
    {
        public MatchupModule(IUserService userService)
        {
            
        }
    }
}
