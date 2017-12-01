using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BobDono.DataAccess.Database;
using BobDono.Interfaces;
using BobDono.Interfaces.Services;
using BobDono.Models.Entities;

namespace BobDono.DataAccess.Services
{
    public class MatchupService : ServiceBase<Matchup,IMatchupService>, IMatchupService
    {
        public MatchupService()
        {

        }

        private MatchupService(BobDatabaseContext dbContext, bool saveOnDispose) : base(dbContext, saveOnDispose)
        {

        }

        public override IMatchupService ObtainLifetimeHandle(IDatabaseCommandExecutionContext executionContext, bool saveOnDispose = true)
        {
            return new MatchupService(executionContext.Context as BobDatabaseContext, saveOnDispose);
        }

        public Task<Matchup> GetMatchup(long matchupId)
        {
            return FirstAsync(matchup => matchup.Id == matchupId);
        }
    }
}
