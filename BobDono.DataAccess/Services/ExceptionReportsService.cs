using System;
using System.Collections.Generic;
using System.Text;
using BobDono.DataAccess.Database;
using BobDono.Interfaces;
using BobDono.Interfaces.Services;
using BobDono.Models.Entities;

namespace BobDono.DataAccess.Services
{
    public class ExceptionReportsService : ServiceBase<ExceptionReport,IExceptionReportsService> , IExceptionReportsService
    {
        public ExceptionReportsService()
        {

        }

        private ExceptionReportsService(BobDatabaseContext dbContext, bool saveOnDispose) : base(dbContext,saveOnDispose)
        {

        }

        public override IExceptionReportsService ObtainLifetimeHandle(IDatabaseCommandExecutionContext executionContext,
            bool saveOnDispose = true)
        {
            return new ExceptionReportsService(executionContext.Context as BobDatabaseContext, saveOnDispose);
        }
    }
}
