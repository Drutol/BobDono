using System.Linq;
using System.Threading.Tasks;
using BobDono.DataAccess.Database;
using BobDono.Interfaces;
using BobDono.Interfaces.Services;
using BobDono.Models.Entities;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;

namespace BobDono.DataAccess.Services
{

    public class UserService : ServiceBase<User,IUserService> , IUserService
    {
        public UserService()
        {
            
        }

        private UserService(BobDatabaseContext dbContext, bool saveOnDispose) : base(dbContext,saveOnDispose)
        {

        }

        public async Task<User> GetOrCreateUser(DiscordUser discordUser)
        {
            var user = await FirstAsync(u => u.DiscordId == discordUser.Id);

            user.AvatarUrl = discordUser.AvatarUrl;

            if (user != null)
                return user;

            user = new User
            {
                DiscordId = discordUser.Id,
                Name = discordUser.Username
            };

            Add(user);

            return user;
        }





#if DEBUG
            public const ulong MyId = 343050467879813140; //ranko
#else
            public const ulong MyId = 377859054464401408; //bob
#endif

        public async Task<User> GetBobUser()
        {
            var user = await FirstAsync(u => u.DiscordId == MyId);

            user = new User
            {
                DiscordId = MyId,
                Name = "BobDono"
            };

            Add(user);

            return user;
        }

        public override IUserService ObtainLifetimeHandle(IDatabaseCommandExecutionContext executionContext, bool saveOnDispose = true)
        {
            return new UserService(executionContext.Context as BobDatabaseContext, saveOnDispose);
        }
    }
}
