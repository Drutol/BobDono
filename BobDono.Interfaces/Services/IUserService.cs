﻿using System.Threading.Tasks;
using BobDono.Models.Entities;
using DSharpPlus.Entities;

namespace BobDono.Interfaces.Services
{
    public interface IUserService : IServiceBase<User>
    {
        Task<User> GetOrCreateUser(DiscordUser discordUser);
    }
}