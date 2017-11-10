using System;
using System.Linq;
using BobDono.Interfaces;
using BobDono.Interfaces.Services;

namespace BobDono.DataAccess.Services
{
    public interface IIncludeConfigurator<TEntity, TService>
        where TEntity : class
        where TService : class, IServiceBase<TEntity, TService>
    {
       
        void Commit();
        IIncludeConfigurator<TEntity, TService> ExtendChain(EntityIncludeDelegate<TEntity> chain);
        IIncludeConfigurator<TEntity, TService> WithChain(EntityIncludeDelegate<TEntity> chain);
        IIncludeConfigurator<TEntity, TService> IgnoreDefaultServiceIncludes();
    }
}