using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;

namespace BobDono.Core.Extensions
{
    public static class DiscordClientExtensions
    {
        public const string ElectionsCategoryName = "Elections";

        private static DiscordGuild _nullsGuild;
        private static DiscordChannel _electionCategoryChannel;

        public static DiscordGuild GetNullsGuild(this DiscordClient client)
        {
#if DEBUG
            return _nullsGuild ??
                   (_nullsGuild = client.Guilds.First(pair => pair.Value.Id == 343060137164144642).Value);
#else
            return _nullsGuild ??
                   (_nullsGuild = client.Guilds.First(pair => pair.Value.Id == 317924870950223872).Value);
#endif

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

        public static bool IsMe(this DiscordUser user)
        {
#if DEBUG
            return user.Id == 343050467879813140; //ranko
#else
            return user.Id == 377859054464401408; //bob
#endif

        }
    }
}
