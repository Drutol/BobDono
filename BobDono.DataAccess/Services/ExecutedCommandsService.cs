using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BobDono.DataAccess.Database;
using BobDono.Interfaces.Services;
using BobDono.Models.Entities.Stats;

namespace BobDono.DataAccess.Services
{
    public class ExecutedCommandsService : ServiceBase<ExecutedCommand,IExecutedCommandsService> , IExecutedCommandsService
    {
        public ExecutedCommandsService()
        {

        }

        private ExecutedCommandsService(BobDatabaseContext dbContext, bool saveOnDispose) : base(dbContext, saveOnDispose)
        {

        }

        public List<IGrouping<int, ExecutedCommand>> GetGrouping(Func<ExecutedCommand, int> selector, bool existed)
        {
            return Context.Set<ExecutedCommand>().Where(command => command.Existed == existed).AsEnumerable().GroupBy(selector).ToList();
        }
    }
}
