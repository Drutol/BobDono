using System;
using System.Collections.Generic;
using System.Text;

namespace BobDono.Interfaces.Services
{
    public interface IServiceFactory<out TService> where TService : class, IServiceFactory<TService>
    {
        TService ObtainLifetimeHandle(IDatabaseCommandExecutionContext executionContext, bool saveOnDispose = true);
        TService ObtainLifetimeHandle(bool saveOnDispose = true);
    }
}
