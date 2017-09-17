using System.Linq;
using System.Threading.Tasks;
using BobDono.Attributes;
using BobDono.BL;
using BobDono.Utils;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace BobDono.Modules
{
    [Module(Name = "Waifu",Description = "Retrieve character data from MAL.")]
    public class WaifuModule
    {
        [CommandHandler(Regex = @"waifus", HumanReadableCommand = "waifus",HelpText = "Shows favourite character of the caller.")]
        public async Task DisplayFavouriteCharacterForUser(MessageCreateEventArgs args)
        {
            if (DiscordMalMapper.TryGetMalUsername(args.Author.Id, out string userName))
            {
                await args.Channel.SendMessageAsync(await GetCharactersReplyForUser(userName));
            }
        }

        [CommandHandler(Regex = @"waifus (<@\d+>|\w+)", HumanReadableCommand = "waifus <username>",HelpText = "Shows favourite character of specified user.")]
        public async Task DisplayInfoForUserCommand(MessageCreateEventArgs args)
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

        [CommandHandler(Regex = @"waifu (\w+|\s)+", HumanReadableCommand = "waifu <name> [show]")]
        public async Task GetCharacterInfo(MessageCreateEventArgs args)
        {
            var messageArgs = args.Message.Content.Split(' ');
            var searchResults = await BotContext.CharactersSearchQuery.GetSearchResults(messageArgs[1]);
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

            var details = await BotContext.CharacterDetailsQuery.GetCharacterDetails(int.Parse(id));

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
            var data = await BotContext.ProfileQuery.GetProfileData(malUsername);
            return $"Waifus:\n{string.Join("\n", data.FavouriteCharacters.Select(character => character.Name))}";
        }

    }

}
