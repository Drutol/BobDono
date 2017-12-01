using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BobDono.Models.Entities;

namespace BobDono.Interfaces.Services
{
    public interface IMatchupService : IServiceBase<Matchup,IMatchupService>
    {
        Task<Matchup> GetMatchup(long matchupId);
    }
}
