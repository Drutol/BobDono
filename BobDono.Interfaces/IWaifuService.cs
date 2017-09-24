using System.Threading.Tasks;
using BobDono.Models.Entities;

namespace BobDono.Interfaces
{
    public interface IWaifuService
    {
        Task<Waifu> GetOrCreateWaifu(string malId);
    }
}