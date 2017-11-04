using BobDono.Models.Entities;

namespace BobDono.Interfaces.Services
{
    public interface IContenderService : IServiceBase<WaifuContender>
    {
        WaifuContender CreateContender(User user, Waifu waifu, Election election,
            string customImage = null);
    }
}