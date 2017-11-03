using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using BobDono.DataAccess.Database;
using BobDono.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BobDono.DataAccess.Services
{
    public abstract class ServiceBase<T> : IServiceBase<T> where T : class
    {
        protected bool SaveOnDispose;
        protected BobDatabaseContext Context;

        internal ServiceBase()
        {
            
        }

        protected virtual IQueryable<T> Include(DbSet<T> query)
        {
            return query;
        }

        internal ServiceBase(BobDatabaseContext dbContext, bool saveOnDispose)
        {
            Context = dbContext;
            SaveOnDispose = saveOnDispose;
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
            return Include(Context.Set<T>()).ToList();
        }

        public Task<List<T>> GetAllAsync()
        {
            return Include(Context.Set<T>()).ToListAsync();
        }

        public Task<List<T>> GetAllWhereAsync(Predicate<T> predicate)
        {
            return Include(Context.Set<T>()).Where(client => predicate(client)).ToListAsync();
        }

        public async Task<T> FirstAsync(Predicate<T> predicate)
        {
            try
            {
                return await Include(Context.Set<T>()).FirstAsync(client => predicate(client));
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
                if (SaveOnDispose)
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
    }
}