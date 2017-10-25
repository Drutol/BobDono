using System.Collections.Generic;
using System.Threading.Tasks;
using BobDono.Models.Entities;

namespace BobDono.Interfaces
{
    public interface IElectionService
    {
        Task<Election> GetElection(long id);
        Task<Election> CreateElection(Election election, User user);
        void Remove(Election election);
        IEnumerable<Election> GetAll();
        Task<IDelegatedEntityUpdate<Election>> ObtainElectionUpdate(long id);
        IDelegatedEntityUpdate<Election> ObtainElectionUpdate(Election election);
        void Update(Election election);
    }
}