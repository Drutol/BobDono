using System.Threading.Tasks;
using BobDono.Models.Entities;

namespace BobDono.Interfaces
{
    public interface IContenderService
    {
        Task<WaifuContender> CreateContender(User user, Waifu waifu, Election election,
            string customImage = null);
    }
}