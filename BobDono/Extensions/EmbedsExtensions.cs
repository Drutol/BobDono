using System;
using System.Collections.Generic;
using System.Text;
using BobDono.Entities;
using DSharpPlus.Entities;

namespace BobDono.Extensions
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
