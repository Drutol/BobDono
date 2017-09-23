using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;

namespace BobDono.Utils
{
    public static class DiscordClientExtensions
    {
        public const string ElectionsCategoryName = "Elections";

        private static DiscordGuild _nullsGuild;
        private static DiscordChannel _electionCategoryChannel;

        public static DiscordGuild GetNullsGuild(this DiscordClient client)
        {
            return _nullsGuild ??
                   (_nullsGuild = client.Guilds.First(pair => pair.Value.Id == 317924870950223872 || pair.Value.Id == 343060137164144642).Value);
        }

        public static async Task<DiscordChannel> GetElectionsCategory(this DiscordGuild guild)
        {
            if (_electionCategoryChannel != null)
                return _electionCategoryChannel;
            var channel = (await guild.GetChannelsAsync()).FirstOrDefault(discordChannel =>
                discordChannel.IsCategory && discordChannel.Name == ElectionsCategoryName);
            if (channel != null)
            {
                _electionCategoryChannel = channel;
                return _electionCategoryChannel;
            }
            _electionCategoryChannel = await guild.CreateChannelAsync(ElectionsCategoryName, ChannelType.Category);
            return _electionCategoryChannel;
        }
    }
}
