using System.Collections.Generic;
using System.Threading.Tasks;
using BobDono.Models.MalHell;

namespace BobDono.Interfaces
{
    public interface ICharactersSearchQuery
    {
        Task<List<AnimeCharacter>> GetSearchResults(string query);
    }
}