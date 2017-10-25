using System.Collections.Generic;
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
        
        
        public static IEnumerable<DiscordEmbed> GetEmbed(this Bracket stage)
        {
            if (stage.SecondContender != null)
            {
                yield return GetEmbed(stage.FirstContender, 1);
                yield return GetEmbed(stage.SecondContender, 2);
            }
            else
            {
                yield return GetEmbed(stage.FirstContender, 0);
            }

            DiscordEmbed GetEmbed(WaifuContender contender, int contenderNumber)
            {
                if (contenderNumber > 0)
                {
                    var builder = new DiscordEmbedBuilder
                    {
                        Color = DiscordColor.Brown,
                        Title = $"Bracket #{stage.Number}, Contender #{contenderNumber}",
                        ImageUrl = contender.CustomImageUrl ?? contender.Waifu.ImageUrl
                    };
                    return builder.Build();
                }

                return new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Brown,
                    Title = $"There's odd number of contenders, this lucky contender wins just like this.",
                    ImageUrl = contender.CustomImageUrl ?? contender.Waifu.ImageUrl
                }.Build();

            }
        }
    }
}
