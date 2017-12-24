using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BobDono.Models.Entities.Stats;

namespace BobDono.Interfaces.Services
{
    public interface IExecutedCommandsService : IServiceBase<ExecutedCommand,IExecutedCommandsService>
    {
        List<IGrouping<int, ExecutedCommand>> GetGrouping(Func<ExecutedCommand, int> selector, bool existed);
    }
}
