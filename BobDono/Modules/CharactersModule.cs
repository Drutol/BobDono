using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BobDono.Core.Attributes;
using BobDono.Core.BL;
using BobDono.Interfaces;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace BobDono.Modules
{
    [Module(Name = "Characters",Description = "Retrieve character details from MAL.")]
    public class CharactersModule
    {
        private readonly ICharactersSearchQuery _charactersSearchQuery;
        private readonly IProfileQuery _profileQuery;
        private readonly ICharacterDetailsQuery _characterDetailsQuery;

        public CharactersModule(ICharactersSearchQuery charactersSearchQuery, IProfileQuery profileQuery, ICharacterDetailsQuery characterDetailsQuery)
        {
            _charactersSearchQuery = charactersSearchQuery;
            _profileQuery = profileQuery;
            _characterDetailsQuery = characterDetailsQuery;
        }

        [CommandHandler(Regex = @"character", HumanReadableCommand = "character", HelpText = "Shows favourite character of the caller.")]
        public async Task DisplayFavouriteCharacterForUser(MessageCreateEventArgs args, ICommandExecutionContext context)
        {
            if (DiscordMalMapper.TryGetMalUsername(args.Author.Id, out string userName))
            {
                await args.Channel.SendMessageAsync(await GetCharactersReplyForUser(userName));
            }
        }

        [CommandHandler(Regex = @"characters (<@\d+>|\w+)", HumanReadableCommand = "characters <username>", HelpText = "Shows favourite character of specified user.")]
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

        [CommandHandler(Regex = @"character (\w+|\s)+", HumanReadableCommand = "character <name> [show]")]
        public async Task GetCharacterInfo(MessageCreateEventArgs args, ICommandExecutionContext context)
        {
            var messageArgs = args.Message.Content.Split(' ');
            var searchResults = await _charactersSearchQuery.GetSearchResults(messageArgs[1]);
            if (!searchResults?.Any() ?? true)
            {
                await args.Channel.SendMessageAsync("Nothing found... maybe your waifu doesn't exist yet?");
                return;
            }

            string id = null;
            if (messageArgs.Length == 3)
                id = searchResults.FirstOrDefault(character => character.ShowName.ToLower().Contains(messageArgs[2]))?.Id;

            if (id == null)
                id = searchResults.First().Id;

            var details = await _characterDetailsQuery.GetCharacterDetails(int.Parse(id));

            var builder = new DiscordEmbedBuilder
            {
                Title = details.Name,
                Description = details.Content,
                ThumbnailUrl = details.ImgUrl,
                Color = DiscordColor.Brown
            };



            await args.Channel.SendMessageAsync(null, false, builder.Build());
        }

        private async Task<string> GetCharactersReplyForUser(string malUsername)
        {
            var data = await _profileQuery.GetProfileData(malUsername);
            return $"Waifus:\n{string.Join("\n", data.FavouriteCharacters.Select(character => character.Name))}";
        }
    }
}
