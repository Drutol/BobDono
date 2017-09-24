using System.Threading.Tasks;
using BobDono.Models.Entities;

namespace BobDono.Interfaces
{
    public interface IElectionService
    {
        Task<Election> GetElection(long id);
        Task<Election> CreateElection(Election election, User user);
    }
}