using System;
using System.Collections.Generic;
using System.Text;
using BobDono.Core.Attributes;
using BobDono.Core.Utils;

namespace BobDono.Core.Interfaces
{
    public interface IBotContext
    {
        Dictionary<ModuleAttribute, List<CommandHandlerAttribute>> Commands { get; set; }
        event Delegates.MessageDelegate NewPrivateMessage;
    }
}
