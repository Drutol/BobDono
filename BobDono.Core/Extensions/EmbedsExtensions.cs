using System.Collections.Generic;
using BobDono.Models.Entities;
using DSharpPlus.Entities;

namespace BobDono.Core.Extensions
{
    public static class EmbedsExtensions
    {
        public static DiscordEmbed GetEmbed(this WaifuContender contender)
        {
            return GetEmbedBuilder(contender).Build();
        }

        public static DiscordEmbedBuilder GetEmbedBuilder(this WaifuContender contender)
        {
            var builder = new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor {Name = contender.Proposer.Name},
                Color = DiscordColor.Gray,
                Description = contender.Waifu.Description,
                ThumbnailUrl = contender.CustomImageUrl ?? contender.Waifu.ImageUrl,
                Title = $"Contender: {contender.Waifu.Name}"
            };

            builder.WithUrl($"https://myanimelist.net/character/{contender.Waifu.MalId}");

            return builder;
        }    
        
        
        public static IEnumerable<DiscordEmbed> GetEmbeds(this Bracket stage)
        {
            yield return GetEmbed(stage.FirstContender, 1);
            if (stage.SecondContender != null)
                yield return GetEmbed(stage.SecondContender, 2);
            if (stage.ThirdContender != null)
                yield return GetEmbed(stage.FirstContender, 3);
            
            DiscordEmbed GetEmbed(WaifuContender contender, int contenderNumber)
            {
                var builder = new DiscordEmbedBuilder
                {

                    Color = DiscordColor.Brown,
                    Title = $"Bracket #{stage.Number}, Contender #{contenderNumber}",
                    ImageUrl = contender.CustomImageUrl ?? contender.Waifu.ImageUrl
                };
                return builder.Build();
            }
        }
    }
}
