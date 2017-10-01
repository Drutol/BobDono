using System;
using System.Collections.Generic;
using System.Text;

namespace BobDono.Interfaces
{
    public interface IDelegatedEntityUpdate<out TEntity> : IDisposable
    {
        TEntity Entity { get; }
    }
}
