using System.Threading.Tasks;
using BobDono.Models.MalHell;

namespace BobDono.Interfaces.Queries
{
    public interface ICharacterDetailsQuery
    {
        Task<CharacterDetailsData> GetCharacterDetails(int id);
    }
}