using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BobDono.Models.Entities;

namespace BobDono.Interfaces
{
    public interface IElectionProvider
    {
        IQueryable<Election> Elections { get; }
    }
}
