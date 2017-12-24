using System;
using System.Collections.Generic;
using System.Text;
using BobDono.DataAccess.Database;
using BobDono.Interfaces;
using BobDono.Interfaces.Services;
using BobDono.Models.Entities;

namespace BobDono.DataAccess.Services
{
    public class ElectionThemeService : ServiceBase<ElectionTheme,IElectionThemeService> , IElectionThemeService
    {
        public ElectionThemeService()
        {

        }

        private ElectionThemeService(BobDatabaseContext dbContext, bool saveOnDispose) : base(dbContext, saveOnDispose)
        {

        }
    }
}
