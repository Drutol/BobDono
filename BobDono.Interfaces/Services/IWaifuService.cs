using System.Threading.Tasks;
using BobDono.Models.Entities;

namespace BobDono.Interfaces.Services
{
    public interface IWaifuService : IServiceBase<Waifu,IWaifuService>
    {
        Task<Waifu> GetOrCreateWaifu(string malId, bool force = false);
    }
}