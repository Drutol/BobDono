using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;

namespace BobDono.Core.Extensions
{
    public static class DiscordClientExtensions
    {
        public enum ChannelCategory
        {
            Elections,
            ElectionsMeta,
            Matchups
        }

        public const string ElectionsCategoryName = "Elections";
        public const string ElectionsMetaCategoryName = "ElectionsMeta";
        public const string MatchupsCategoryName = "Matchups";

        private static DiscordGuild _nullsGuild;

        private static Dictionary<ChannelCategory, DiscordChannel> _categoryChannels =
            new Dictionary<ChannelCategory, DiscordChannel>();

        public static DiscordGuild GetNullsGuild(this DiscordClient client)
        {
#if DEBUG
            return _nullsGuild ??
                   (_nullsGuild = client.Guilds.First(pair => pair.Value.Id == 343060137164144642).Value);
#else
            return _nullsGuild ??
                   (_nullsGuild = client.Guilds.First(pair => pair.Value.Id == 343060137164144642).Value);
#endif

        }

        public static async Task<DiscordChannel> GetCategoryChannel(this DiscordGuild guild,ChannelCategory category)
        {
            if (_categoryChannels.ContainsKey(category))
                return _categoryChannels[category];

            string key;
            switch (category)
            {
                case ChannelCategory.Elections:
                    key = ElectionsCategoryName;
                    break;
                case ChannelCategory.ElectionsMeta:
                    key = ElectionsMetaCategoryName;
                    break;
                case ChannelCategory.Matchups:
                    key = MatchupsCategoryName;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(category), category, null);
            }

            var ch = await GetCategoryChannel(guild, key);
            _categoryChannels[category] = ch;

            return ch;
        }


        private static async Task<DiscordChannel> GetCategoryChannel(DiscordGuild guild, string key)
        {
            var channel = (await guild.GetChannelsAsync()).FirstOrDefault(discordChannel =>
                discordChannel.IsCategory && discordChannel.Name == key);
            if (channel != null)
                return channel;
            return await guild.CreateChannelAsync(key, ChannelType.Category);
        }

        public static bool IsMe(this DiscordUser user)
        {
#if DEBUG
            return user.Id == 377859054464401408; // 343050467879813140; //ranko
#else
            return user.Id == 377859054464401408; //bob
#endif
        }

        public static bool IsAuthenticated(this DiscordUser user)
        {
            return user.Id == 74458088760934400;
        }
    }
}
