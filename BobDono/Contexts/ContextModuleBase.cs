using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using BobDono.Core;
using BobDono.Core.BL;
using BobDono.Core.Interfaces;
using BobDono.Interfaces;

namespace BobDono.Contexts
{
    public abstract class ContextModuleBase : IModule
    {
        public abstract ulong? ChannelIdContext { get; }

        


        protected ContextModuleBase()
        {
            ResourceLocator.BotBackbone.Modules[GetType()].Contexts.Add(this);
        }
    }
}
