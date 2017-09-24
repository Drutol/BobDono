using System;
using System.Collections.Generic;
using System.Text;
using BobDono.Core.Attributes;
using BobDono.Core.BL;
using BobDono.Core.Utils;

namespace BobDono.Core.Interfaces
{
    public interface IBotBackbone
    {
        Dictionary<Type, ModuleAttribute> Modules { get; }
        List<HandlerEntry> Handlers { get; }
        void Initialize();
    }
}
