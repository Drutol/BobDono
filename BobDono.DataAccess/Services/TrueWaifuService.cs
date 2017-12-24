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
    public class TrueWaifuService : ServiceBase<TrueWaifu,ITrueWaifuService> , ITrueWaifuService
    {
        public TrueWaifuService()
        {

        }

        private TrueWaifuService(BobDatabaseContext dbContext, bool saveOnDispose) : base(dbContext, saveOnDispose)
        {
            
        }

        protected override IQueryable<TrueWaifu> Include(IQueryable<TrueWaifu> query)
        {
            return query.Include(w => w.Waifu);
        }
    }
}
