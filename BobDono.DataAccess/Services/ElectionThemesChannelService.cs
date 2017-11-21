using System;
using System.Collections.Generic;
using System.Text;
using BobDono.DataAccess.Database;
using BobDono.Interfaces;
using BobDono.Interfaces.Services;
using BobDono.Models.Entities.Simple;

namespace BobDono.DataAccess.Services
{
    public class ElectionThemesChannelService : ServiceBase<ElectionThemeChannel,IElectionThemesChannelService> , IElectionThemesChannelService
    {

        public ElectionThemesChannelService()
        {
            
        }

        private ElectionThemesChannelService(BobDatabaseContext dbContext, bool saveOnDispose) : base(dbContext, saveOnDispose)
        {

        }

        public override IElectionThemesChannelService ObtainLifetimeHandle(IDatabaseCommandExecutionContext executionContext, bool saveOnDispose = true)
        {
            return new ElectionThemesChannelService(executionContext.Context as BobDatabaseContext, saveOnDispose);
        }
    }
}
