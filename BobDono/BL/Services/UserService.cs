using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BobDono.Database;
using BobDono.Entities;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;

namespace BobDono.BL
{
    public class UserService
    {
        #region Singleton

        private UserService()
        {

        }

        public static UserService Instance { get; } = new UserService();

        #endregion

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
