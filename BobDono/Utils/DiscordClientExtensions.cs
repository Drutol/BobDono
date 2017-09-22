using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DSharpPlus;
using DSharpPlus.Entities;

namespace BobDono.Utils
{
    public static class DiscordClientExtensions
    {
        private static DiscordGuild _nullsGuild;

        public static DiscordGuild GetNullsGuild(this DiscordClient client)
        {
            return _nullsGuild ??
                   (_nullsGuild = client.Guilds.First(pair => pair.Value.Id == 317924870950223872).Value);
        }
    }
}
