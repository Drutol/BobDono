using BobDono.Models.Entities;
using DSharpPlus.Entities;

namespace BobDono.Core.Extensions
{
    public static class EmbedsExtensions
    {
        public static DiscordEmbed GetEmbed(this WaifuContender contender)
        {
            var builder = new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor {Name = contender.Proposer.Name},
                Color = DiscordColor.Gray,
                Description = contender.Waifu.Description,
                ThumbnailUrl = contender.CustomImageUrl ?? contender.Waifu.ImageUrl,
                Title = $"Contender: {contender.Waifu.Name}",
            };

            return builder.Build();
        }
    }
}
