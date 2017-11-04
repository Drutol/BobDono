using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BobDono.DataAccess.Database;
using BobDono.Interfaces;
using BobDono.Interfaces.Services;
using BobDono.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BobDono.DataAccess.Services
{
    public class TrueWaifuService : ServiceBase<TrueWaifu> , ITrueWaifuService
    {
        public TrueWaifuService()
        {

        }

        protected override IQueryable<TrueWaifu> Include(DbSet<TrueWaifu> query)
        {
            return query.Include(w => w.Waifu);
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
