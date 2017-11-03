using System;
using System.Collections.Generic;
using System.Text;
using BobDono.DataAccess.Database;
using BobDono.Interfaces;
using BobDono.Models.Entities;

namespace BobDono.DataAccess.Services
{
    public class TrueWaifuService : ServiceBase<TrueWaifu> , ITrueWaifuService
    {
        public TrueWaifuService()
        {

        }

        private TrueWaifuService(BobDatabaseContext dbContext, bool saveOnDispose) : base(dbContext, saveOnDispose)
        {
            
        }

        public override IServiceBase<TrueWaifu> ObtainLifetimeHandle(ICommandExecutionContext executionContext, bool saveOnDispose = true)
        {
            return new TrueWaifuService(executionContext.Context as BobDatabaseContext, saveOnDispose);
        }
    }
}
