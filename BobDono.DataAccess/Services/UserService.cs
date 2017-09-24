using System.Threading.Tasks;
using BobDono.DataAccess.Database;
using BobDono.Interfaces;
using BobDono.Models.Entities;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;

namespace BobDono.DataAccess.Services
{

    public class UserService : IUserService
    {
        public async Task<User> GetOrCreateUser(DiscordUser discordUser)
        {
            using (var db = new BobDatabaseContext())
            {
                var user = await db.Users.FirstOrDefaultAsync(u => u.DiscordId == discordUser.Id);
                if (user != null)
                    return user;

                user = new User
                {
                    DiscordId = discordUser.Id,
                    Name = discordUser.Username
                };

                db.Users.Add(user);
                await db.SaveChangesAsync();

                return user;
            }
        }
    }
}
