using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BobDono.Core.Attributes;
using BobDono.Core.BL;
using BobDono.DataAccess.Services;
using BobDono.Interfaces;
using BobDono.Interfaces.Queries;
using BobDono.Interfaces.Services;
using BobDono.Models.MalHell;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;

namespace BobDono.Modules
{
    [Module(Name = "Characters",Description = "Retrieve character details from MAL.")]
    public class CharactersModule
    {
        private readonly List<CharacterDetailsData> _charactersCache = new List<CharacterDetailsData>();

        private readonly ICharactersSearchQuery _charactersSearchQuery;
        private readonly IServiceFactory<ITrueWaifuService> _trueWaifuService;
        private readonly IProfileQuery _profileQuery;
        private readonly ICharacterDetailsQuery _characterDetailsQuery;

        public CharactersModule(ICharactersSearchQuery charactersSearchQuery,
            IServiceFactory<ITrueWaifuService> trueWaifuService, IProfileQuery profileQuery,
            ICharacterDetailsQuery characterDetailsQuery)
        {
            _charactersSearchQuery = charactersSearchQuery;
            _trueWaifuService = trueWaifuService;
            _profileQuery = profileQuery;
            _characterDetailsQuery = characterDetailsQuery;
        }

        [CommandHandler(Regex = @"characters", HumanReadableCommand = "characters", HelpText = "Shows favourite character of the caller.")]
        public async Task DisplayFavouriteCharacterForUser(MessageCreateEventArgs args, ICommandExecutionContext context)
        {
            if (DiscordMalMapper.TryGetMalUsername(args.Author.Id, out string userName))
            {
                await args.Channel.SendMessageAsync(await GetCharactersReplyForUser(userName));
            }
        }

        [CommandHandler(Regex = @"characters (<@\d+>|\w+|<@!\d+>)", HumanReadableCommand = "characters <username>", HelpText = "Shows favourite character of specified user.")]
        public async Task DisplayInfoForUserCommand(MessageCreateEventArgs args, ICommandExecutionContext context)
        {
            string response = null;
            if (args.Message.MentionedUsers.Any())
            {
                if (DiscordMalMapper.TryGetMalUsername(args.Author.Id, out string userName))
                {
                    response = await GetCharactersReplyForUser(userName);
                }
            }
            else
            {
                response = await GetCharactersReplyForUser(DiscordMalMapper.TryGetMalUsername(args.Message.Content.Split(' ').Last()));
            }
            await args.Channel.SendMessageAsync(response);
        }

        [CommandHandler(Regex = "character \"([^\"]*)\"", HumanReadableCommand = "character <\"name\">", HelpText = "Gets character info from MAL, you can type in multiple words into quotes.")]
        public async Task CharacterWithQuotes(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            var groups = Regex.Matches(args.Message.Content,@"character ""([^""]*)""");

            await SearchForCharacter(groups[0].Groups[1].Value, null, null, args.Channel,
                executionContext);
        }

        [CommandHandler(Regex = @"character (\w+|\s)+", HumanReadableCommand = "character <name>/<id> [show]",HelpText = "Gets character info, one word per parameter.")]
        public async Task GetCharacterInfo(MessageCreateEventArgs args, ICommandExecutionContext context)
        {
            var messageArgs = args.Message.Content.Split(' ');

            string id = null;

            if (int.TryParse(messageArgs[1], out int parsed))
                id = messageArgs[1];

            await SearchForCharacter(messageArgs[1], messageArgs.Length == 3 ? messageArgs[2] : null, id, args.Channel,
                context);

        }

        private async Task SearchForCharacter(string name, string show, string id,DiscordChannel channel,ICommandExecutionContext context)
        {
            if (id == null)
            {
                var searchResults = await _charactersSearchQuery.GetSearchResults(name);
                if (!searchResults?.Any() ?? true)
                {
                    await channel.SendMessageAsync("Nothing found... maybe your waifu doesn't exist yet?");
                    return;
                }

                if (show != null)
                    id = searchResults
                        .FirstOrDefault(character => character.ShowName.ToLower().Contains(show))
                        ?.Id;

                if (id == null)
                    id = searchResults.First().Id;
            }

            var intId = int.Parse(id);

            var details = _charactersCache.FirstOrDefault(data => data.Id == intId);

            if (details == null)
            {
                details = await _characterDetailsQuery.GetCharacterDetails(intId);
                _charactersCache.Add(details);
            }

            await DisplayCharacterDetails(details, context, channel);
        }

        private async Task DisplayCharacterDetails(CharacterDetailsData details, ICommandExecutionContext context,
            DiscordChannel channel)
        {
            string content = null;
            content = details.Content.Length > 1300 ? $"{details.Content.Substring(0, 1300)}..." : details.Content;

            var builder = new DiscordEmbedBuilder
            {
                Title = details.Name,
                Description = content,
                ThumbnailUrl = details.ImgUrl,
                Color = DiscordColor.Brown,
                Url = $"https://myanimelist.net/character/{details.Id}",
                Footer = new DiscordEmbedBuilder.EmbedFooter {Text = $"Id: {details.Id}"}
            };

            if (details.VoiceActors.Any())
                builder.AddField("Voice Actors:",
                    string.Join("\n", details.VoiceActors.Take(3).Select(person => $"{person.Name} *({person.Id})*")));
            if (details.Animeography.Any())
                builder.AddField("Animeography:",
                    string.Join("\n", details.Animeography.Take(3).Select(show => $"{show.Title} *({show.Id})*")));
            if (details.Mangaography.Any())
                builder.AddField("Mangaography:",
                    string.Join("\n", details.Mangaography.Take(3).Select(manga => $"{manga.Title} *({manga.Id})*")));

            using (var waifuService = _trueWaifuService.ObtainLifetimeHandle(context))
            {
                waifuService.ConfigureIncludes().WithChain(q => q.Include(w => w.Waifu).Include(w => w.User)).Commit();
                var trueWaifus = waifuService.GetAll()
                    .Where(waifu => waifu.Waifu != null && waifu.Waifu.MalId == details.Id.ToString()).ToList();

                if (trueWaifus.Any())
                {
                    builder.AddField("This is waifu of:",
                        string.Join("\n", trueWaifus.Select(waifu => $"{waifu.User.Name}")));

                    builder.ImageUrl = trueWaifus.FirstOrDefault(waifu => waifu.FeatureImage != null)?.FeatureImage;
                }
            }

            await channel.SendMessageAsync(null, false, builder.Build());
        }

        private async Task<string> GetCharactersReplyForUser(string malUsername)
        {
            var data = await _profileQuery.GetProfileData(malUsername);
            return $"{malUsername}'s favourite characters:\n{string.Join("\n", data.FavouriteCharacters.Select(character => character.Name))}";
        }
    }
}
