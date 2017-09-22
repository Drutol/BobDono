using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using BobDono.Attributes;
using BobDono.BL;
using BobDono.Interfaces;

namespace BobDono.Contexts
{
    public abstract class ContextModuleBase : IModule
    {
        public abstract ulong? ChannelIdContext { get; }

        protected ContextModuleBase()
        {
            BotBackbone.Instance.Modules[GetType()].Contexts.Add(this);
        }
    }
}
