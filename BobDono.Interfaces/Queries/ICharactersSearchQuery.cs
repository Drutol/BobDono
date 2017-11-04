using System.Collections.Generic;
using System.Threading.Tasks;
using BobDono.Models.MalHell;

namespace BobDono.Interfaces.Queries
{
    public interface ICharactersSearchQuery
    {
        Task<List<AnimeCharacter>> GetSearchResults(string query);
    }
}