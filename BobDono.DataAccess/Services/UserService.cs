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
            var user = await Include(Context.Users).FirstOrDefaultAsync(u => u.DiscordId == discordUser.Id);

            if (user != null)
                return user;

            user = new User
            {
                DiscordId = discordUser.Id,
                Name = discordUser.Username
            };

            Context.Users.Add(user);

            return user;
        }

        public override IUserService ObtainLifetimeHandle(IDatabaseCommandExecutionContext executionContext, bool saveOnDispose = true)
        {
            return new UserService(executionContext.Context as BobDatabaseContext, saveOnDispose);
        }
    }
}
