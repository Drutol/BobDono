using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BobDono.Interfaces
{
    public delegate IQueryable<TEntity> EntityIncludeDelegate<TEntity>(IQueryable<TEntity> query);
}
