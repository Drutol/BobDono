using System;
using System.Collections.Generic;
using System.Text;
using BobDono.DataAccess.Database;
using BobDono.Interfaces;
using BobDono.Interfaces.Services;
using BobDono.Models.Entities;
using BobDono.Models.Entities.Simple;

namespace BobDono.DataAccess.Services
{
    public class HallOfFameChannelService : ServiceBase<HallOfFameChannel,IHallOfFameChannelService> , IHallOfFameChannelService
    {
        public HallOfFameChannelService()
        {

        }

        private HallOfFameChannelService(BobDatabaseContext dbContext, bool saveOnDispose) : base(dbContext, saveOnDispose)
        {

        }
    }
}
