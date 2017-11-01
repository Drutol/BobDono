﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BobDono.Interfaces
{
    public interface IServiceBase<T> : IDisposable where T : class
    {
        void Begin();
        void Finish();

        List<T> GetAll();

        Task<List<T>> GetAllAsync();
        Task<List<T>> GetAllWhereAsync(Predicate<T> predicate);
        Task<T> FirstAsync(Predicate<T> predicate);

        void OneShot(Action expression);
        TReturn OneShot<TReturn>(Func<TReturn> expression);
        Task<TReturn> OneShotAsync<TReturn>(Func<Task<TReturn>> expression);

        T Add(T client);
        void Remove(T client);
        void Update(T client);

        Task SaveChangesAsync();

        IServiceBase<T> ObtainLifetimeHandle(ICommandExecutionContext executionContext, bool saveOnDispose = true);

        TService ObtainLifetimeHandle<TService>(ICommandExecutionContext executionContext, bool saveOnDispose = true)
            where TService : class, IServiceBase<T>;
    }
}
