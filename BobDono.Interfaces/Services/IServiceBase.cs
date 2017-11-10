using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BobDono.DataAccess.Services;

namespace BobDono.Interfaces.Services
{
    public interface IServiceBase<TEntity,TService> : IDisposable 
        where TEntity : class 
        where TService : class , IServiceBase<TEntity,TService>
    {
        List<TEntity> GetAll();

        Task<List<TEntity>> GetAllAsync();
        Task<List<TEntity>> GetAllWhereAsync(Expression<Func<TEntity,bool>> predicate);
        Task<TEntity> FirstAsync(Expression<Func<TEntity, bool>> predicate);

        TEntity Add(TEntity client);
        void Remove(TEntity client);
        void Update(TEntity client);

        Task SaveChangesAsync();
        
        TService ObtainLifetimeHandle(IDatabaseCommandExecutionContext executionContext, bool saveOnDispose = true);
        TService ObtainLifetimeHandle(bool saveOnDispose = true);

        IIncludeConfigurator<TEntity, TService> ConfigureIncludes();
    }
}
