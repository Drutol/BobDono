using System.Threading.Tasks;
using BobDono.Models.Entities;
using DSharpPlus.Entities;

namespace BobDono.Interfaces.Services
{
    public interface IUserService : IServiceBase<User,IUserService>
    {
        Task<User> GetOrCreateUser(DiscordUser discordUser);
        Task<User> GetBobUser();
    }
}