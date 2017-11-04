using System.Threading.Tasks;
using BobDono.Models.Entities;

namespace BobDono.Interfaces.Services
{
    public interface IWaifuService : IServiceBase<Waifu>
    {
        Task<Waifu> GetOrCreateWaifu(string malId);
    }
}