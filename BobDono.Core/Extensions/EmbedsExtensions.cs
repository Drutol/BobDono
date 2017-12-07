using System.Collections.Generic;
using System.Linq;
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
            string content = null;
            content = contender.Waifu.Description.Length > 1300 ? $"{contender.Waifu.Description.Substring(0, 1300)}..." : contender.Waifu.Description;

            var builder = new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor {Name = contender.Proposer.Name, IconUrl = contender.Proposer.AvatarUrl},
                Color = DiscordColor.Gray,
                Description = content,
                ThumbnailUrl = contender.CustomImageUrl ?? contender.Waifu.ImageUrl,
                ImageUrl = contender.FeatureImage,
                Title = $"Contender: {contender.Waifu.Name}",
                Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"WaifuId: {contender.Waifu.MalId}" }              
            };

            if (contender.Waifu.Animeography?.Any() ?? false)
                builder.AddField("Animeography:",
                    string.Join("\n", contender.Waifu.Animeography.Take(2)));
            if (contender.Waifu.Mangaography?.Any() ?? false)
                builder.AddField("Mangaography:",
                    string.Join("\n", contender.Waifu.Mangaography.Take(2)));

            builder.WithUrl($"https://myanimelist.net/character/{contender.Waifu.MalId}");

            return builder;
        }

        public static DiscordEmbed GetEmbedBuilder(this TrueWaifu waifu)
        {
            string content = null;
            content = waifu.Waifu.Description.Length > 1000 ? $"{waifu.Waifu.Description.Substring(0, 1000)}..." : waifu.Waifu.Description;

            var builder = new DiscordEmbedBuilder
            {
                Color = DiscordColor.HotPink,
                Description = content,
                ThumbnailUrl = waifu.ThumbImage ?? waifu.Waifu.ImageUrl,
                Author = new DiscordEmbedBuilder.EmbedAuthor { Name = waifu.User.Name},
                Title = waifu.Waifu.Name,
                Footer = new DiscordEmbedBuilder.EmbedFooter { Text = $"Id: {waifu.Waifu.MalId}"}
            };

            if (waifu.FeatureImage != null)
                builder.WithImageUrl(waifu.FeatureImage);

            if (waifu.Description != null)
                builder.Description = $"{builder.Description}\n\nNote from {waifu.User.Name}:\n{waifu.Description}";

            if (waifu.Waifu.Voiceactors?.Any() ?? false)
                builder.AddField("Voice Actors:",
                    string.Join("\n", waifu.Waifu.Voiceactors));
            if (waifu.Waifu.Animeography?.Any() ?? false)
                builder.AddField("Animeography:",
                    string.Join("\n", waifu.Waifu.Animeography));
            if (waifu.Waifu.Mangaography?.Any() ?? false)
                builder.AddField("Mangaography:",
                    string.Join("\n", waifu.Waifu.Mangaography));

            builder.WithUrl($"https://myanimelist.net/character/{waifu.Waifu.MalId}");
            return builder;
        }    
        
        
        public static IEnumerable<DiscordEmbed> GetEmbeds(this Bracket stage)
        {
            yield return GetEmbed(stage.FirstContender, 1);
            if (stage.SecondContender != null)
                yield return GetEmbed(stage.SecondContender, 2);
            if (stage.ThirdContender != null)
                yield return GetEmbed(stage.ThirdContender, 3);
            
            DiscordEmbed GetEmbed(WaifuContender contender, int contenderNumber)
            {
                var builder = new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Brown,
                    Title = $"Bracket #{stage.Number}, Contender #{contenderNumber}",
                    ImageUrl = contender.CustomImageUrl ?? contender.Waifu.ImageUrl,
                    Footer = new DiscordEmbedBuilder.EmbedFooter { Text = contender.Waifu.Name},
                    Url = $"https://myanimelist.net/character/{contender.Waifu.MalId}"

                };
                return builder.Build();
            }
        }
    }
}
