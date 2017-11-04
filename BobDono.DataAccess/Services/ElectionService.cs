using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BobDono.DataAccess.Database;
using BobDono.DataAccess.Extensions;
using BobDono.Interfaces;
using BobDono.Interfaces.Services;
using BobDono.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace BobDono.DataAccess.Services
{
    public class ElectionService : ServiceBase<Election>, IElectionService
    {
        public ElectionService()
        {
            
        }

        private ElectionService(BobDatabaseContext bobDatabaseContext, bool saveOnDispose) : base(bobDatabaseContext,saveOnDispose)
        {
            
        }

        protected override IQueryable<Election> Include(DbSet<Election> query)
        {
            return query.IncludeAll();
        }

        public async Task<Election> GetElection(long id)
        {
            return await Context.Elections.IncludeAll().FirstOrDefaultAsync(election => election.Id == id);           
        }

        public async Task<Election> CreateElection(Election election, User user)
        {
            election.Author = user;
            user.Elections.Add(election);

            return election;
        }

        public override IServiceBase<Election> ObtainLifetimeHandle(ICommandExecutionContext executionContext, bool saveOnDispose = true)
        {
            return  new ElectionService(executionContext.Context as BobDatabaseContext, saveOnDispose);
        }
    }
}
