using System.Collections.Generic;
using System.Threading.Tasks;
using BobDono.Models.Entities;

namespace BobDono.Interfaces
{
    public interface IElectionService : IServiceBase<Election>
    {
        Task<Election> GetElection(long id);
        Task<Election> CreateElection(Election election, User user);
    }
}