using System;
using System.Linq;
using System.Threading.Tasks;
using BobDono.Core;
using BobDono.Core.Attributes;
using BobDono.Core.Utils;
using BobDono.Interfaces;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace BobDono.Modules
{
    [Module(Hidden = true)]
    public class BobModule
    {
        [CommandHandler(Regex = @"bob")]
        public Task GeneralCommand(MessageCreateEventArgs args, ICommandExecutionContext context)
        {
            var builder = new DiscordEmbedBuilder();
            builder.Title = "Bob-dono the waifu connesieur.";
            builder.Description =
                "Hi, I'm Bob. It's good to see you here. Let's embark on this magical adventure together!\n\nI offer following modules:\n\n";
            builder.Color = DiscordColor.Brown;
            builder.ThumbnailUrl =
                "https://yt3.ggpht.com/-uJh4oSQAwak/AAAAAAAAAAI/AAAAAAAAAAA/AMGKfKvDP3w/s900-c-k-no-mo-rj-c0xffffff/photo.jpg";

            foreach (var module in ResourceLocator.BotContext.Commands)
            {
                if (module.Key.Hidden)
                    continue;

                builder.Description +=
                    $"**{module.Key.Name}**\n*{module.Key.Description ?? " "}*\n{string.Join("\n", module.Value.Where(attribute => !attribute.Debug && !attribute.FallbackCommand).Select(attribute => $"`{attribute.HumanReadableCommand}`"))}\n\n";
            }

            return args.Channel.SendMessageAsync(null, false, builder.Build());
        }

        [CommandHandler(Regex = @"help \w+")]
        public async Task HelpCommand(MessageCreateEventArgs args, ICommandExecutionContext context)
        {
            var cmd = args.Message.Content.Split(' ').Last();
            var handler = ResourceLocator.BotContext.Commands.Values.SelectMany(list => list).FirstOrDefault(attribute => attribute.HumanReadableCommand != null &&
                attribute.HumanReadableCommand.Substring(1).Split(' ').First().Equals(cmd, StringComparison.CurrentCultureIgnoreCase));
            if (handler != null)
            {
                await args.Channel.SendMessageAsync($"Help for `{handler.HumanReadableCommand}`\n\n{handler.HelpText}");
            }
            else
            {
                await args.Channel.SendMessageAsync("No such command found.");
            }
        }
    }
}
