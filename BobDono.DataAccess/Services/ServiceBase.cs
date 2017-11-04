using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BobDono.DataAccess.Database;
using BobDono.Interfaces;
using BobDono.Interfaces.Services;
using Microsoft.EntityFrameworkCore;

namespace BobDono.DataAccess.Services
{
    public abstract class ServiceBase<T> : IServiceBase<T> where T : class
    {
        public class IncludeConfigurator<T> where T :class
        {
            public delegate IQueryable<T> EntityIncludeDelegate(DbSet<T> query);
            private readonly ServiceBase<T> _parent;
            private ServiceBase<T>.IncludeConfigurator<T>.EntityIncludeDelegate _includeChain;

            internal IncludeConfigurator(ServiceBase<T> parent)
            {
                _parent = parent;
            }

            public IncludeConfigurator<T> WithChain(ServiceBase<T>.IncludeConfigurator<T>.EntityIncludeDelegate chain)
            {
                _includeChain = chain;
                return this;
            }

            public void Commit()
            {
                _parent._includeOverride = _includeChain;
            }
        }

        private readonly bool _saveOnDispose;
        protected BobDatabaseContext Context;
        private IncludeConfigurator<T>.EntityIncludeDelegate _includeOverride;

        internal ServiceBase()
        {
            
        }

        private IQueryable<T> InternalInclude(DbSet<T> query)
        {
            var q = _includeOverride == null ? Include(query) : _includeOverride(query);
            _includeOverride = null;
            return q;
        }

        protected virtual IQueryable<T> Include(DbSet<T> query)
        {
            return query;
        }

        internal ServiceBase(BobDatabaseContext dbContext, bool saveOnDispose)
        {
            Context = dbContext;
            _saveOnDispose = saveOnDispose;
        }

        public void Begin()
        {
            Context = new BobDatabaseContext();
        }

        public void Finish()
        {
            Context.Dispose();
        }

        public List<T> GetAll()
        {
            return InternalInclude(Context.Set<T>()).ToList();
        }

        public Task<List<T>> GetAllAsync()
        {
            return InternalInclude(Context.Set<T>()).ToListAsync();
        }

        public Task<List<T>> GetAllWhereAsync(Predicate<T> predicate)
        {
            return InternalInclude(Context.Set<T>()).Where(client => predicate(client)).ToListAsync();
        }

        public async Task<T> FirstAsync(Predicate<T> predicate)
        {
            try
            {
                return await InternalInclude(Context.Set<T>()).FirstAsync(client => predicate(client));
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void OneShot(Action expression)
        {
            try
            {
                Begin();
                expression.Invoke();
            }
            finally
            {
                Context.SaveChanges();
                Dispose();
            }
        }

        public TReturn OneShot<TReturn>(Func<TReturn> expression)
        {
            try
            {
                Begin();
                return expression.Invoke();
            }
            finally
            {
                Dispose();
            }
        }

        public async Task<TReturn> OneShotAsync<TReturn>(Func<Task<TReturn>> expression)
        {
            try
            {
                Begin();
                return await expression();
            }
            finally
            {
                Dispose();
            }
        }


        public T Add(T client)
        {
            return Context.Add(client).Entity;
        }

        public void Remove(T client)
        {
            Context.Set<T>().Remove(client);
        }

        public void Update(T client)
        {
            Context.Set<T>().Update(client);
        }

        public async Task SaveChangesAsync()
        {
            await Context.SaveChangesAsync();
        }

        public abstract IServiceBase<T> ObtainLifetimeHandle(ICommandExecutionContext executionContext, bool saveOnDispose = true);

        public TService ObtainLifetimeHandle<TService>(ICommandExecutionContext executionContext, bool saveOnDispose = true) where TService  : class, IServiceBase<T>
        {
            return ObtainLifetimeHandle(executionContext,saveOnDispose) as TService;
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

        public IncludeConfigurator<T> ConfigureIncludes()
        {
            return new IncludeConfigurator<T>(this);
        }
    }
}