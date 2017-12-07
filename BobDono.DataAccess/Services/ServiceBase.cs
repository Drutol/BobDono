using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BobDono.DataAccess.Database;
using BobDono.Interfaces;
using BobDono.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace BobDono.DataAccess.Services
{
    public abstract class ServiceBase<TEntity,TService> : IServiceBase<TEntity,TService> 
        where TService : class, IServiceBase<TEntity, TService>
        where TEntity : class
    {
        public class IncludeConfigurator<TEntity,TService> : IIncludeConfigurator<TEntity, TService>
            where TService : class, IServiceBase<TEntity, TService>
            where TEntity : class
        {
            private readonly ServiceBase<TEntity,TService> _parent;
            private EntityIncludeDelegate<TEntity> _includeChain;

            internal IncludeConfigurator(ServiceBase<TEntity, TService> parent)
            {
                _parent = parent;
            }

            public IIncludeConfigurator<TEntity, TService> WithChain(EntityIncludeDelegate<TEntity> chain)
            {
                _includeChain = chain;
                return this;
            }

            public IIncludeConfigurator<TEntity, TService> ExtendChain(EntityIncludeDelegate<TEntity> chain)
            {
                _includeChain = query => chain(_parent.Include(query));
                return this;
            }

            public IIncludeConfigurator<TEntity, TService> IgnoreDefaultServiceIncludes()
            {
                _includeChain = IncludeOverride;
                return this;
            }

            public void Commit()
            {
                _parent._includeOverride = _includeChain;
            }

            private IQueryable<TEntity> IncludeOverride(IQueryable<TEntity> query)
            {
                return query;
            }
        }

        private readonly bool _saveOnDispose;
        protected BobDatabaseContext Context;
        private EntityIncludeDelegate<TEntity> _includeOverride;

        internal ServiceBase()
        {

        }

        internal ServiceBase(BobDatabaseContext dbContext, bool saveOnDispose)
        {
            Context = dbContext;
            _saveOnDispose = saveOnDispose;
        }

        private IQueryable<TEntity> InternalInclude(IQueryable<TEntity> query)
        {
            var q = _includeOverride == null ? Include(query) : _includeOverride(query);
            _includeOverride = null;
            return q;
        }

        protected virtual IQueryable<TEntity> Include(IQueryable<TEntity> query)
        {
            return query;
        }

        public List<TEntity> GetAll()
        {
            return InternalInclude(Context.Set<TEntity>()).ToList();
        }

        public Task<List<TEntity>> GetAllAsync()
        {
            return InternalInclude(Context.Set<TEntity>()).ToListAsync();
        }

        public Task<List<TEntity>> GetAllWhereAsync(Expression<Func<TEntity,bool>> predicate)
        {
            return InternalInclude(Context.Set<TEntity>()).Where(predicate).ToListAsync();
        }

        public async Task<TEntity> FirstAsync(Expression<Func<TEntity, bool>> predicate)
        {
            try
            {
                return await InternalInclude(Context.Set<TEntity>()).FirstAsync(predicate);
            }
            catch (Exception e)
            {
                return null;
            }
        }
      
        public TEntity Add(TEntity entity)
        {
            return Context.Add(entity).Entity;
        }

        public void Remove(TEntity entity)
        {
            Context.Set<TEntity>().Remove(entity);
        }

        public void Update(TEntity entity)
        {
            Context.Set<TEntity>().Update(entity);
        }

        public async Task SaveChangesAsync()
        {
            await Context.SaveChangesAsync();
        }

        public abstract TService ObtainLifetimeHandle(IDatabaseCommandExecutionContext executionContext,
            bool saveOnDispose = true);


        public TService ObtainLifetimeHandle(bool saveOnDispose = true)
        {
            return ObtainLifetimeHandle(new CommandExecutionContext(),saveOnDispose);
        }

        public void Dispose()
        {
            try
            {
                if (Context == null)
                    return;
                if (_saveOnDispose)
                    Context.SaveChanges();
                Context.Dispose();
            }
            catch (ObjectDisposedException)
            {

            }
            finally
            {
                Context = null;
            }
        }

        public IIncludeConfigurator<TEntity,TService> ConfigureIncludes()
        {
            return new IncludeConfigurator<TEntity,TService>(this);
        }
    }
}