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
            builder.Title = "Bob-dono the waifu connoisseur.";
            builder.Description =
                "Hi, I'm Bob. It's good to see you here. Let's embark on this magical adventure together!\nI offer following modules:\n";
            builder.Color = DiscordColor.Brown;
            builder.ThumbnailUrl =
                "https://yt3.ggpht.com/-uJh4oSQAwak/AAAAAAAAAAI/AAAAAAAAAAA/AMGKfKvDP3w/s900-c-k-no-mo-rj-c0xffffff/photo.jpg";

            foreach (var module in ResourceLocator.BotContext.Commands)
            {
                if (module.Key.Hidden)
                    continue;

                builder.AddField(module.Key.Name,
                    $"{module.Key.Description ?? " "}\n{string.Join(",", module.Value.Where(attribute => !attribute.Debug && !attribute.FallbackCommand).Select(attribute => $"`{attribute.HumanReadableCommand}`"))}\n");
            }

            var about = "\n" +
                        $"You can call `{CommandHandlerAttribute.CommandStarter}help <command>` to get more info.";

            about += "\n\n" +
                     "So maybe now about me... I'm a bot... Bob The Bot... nice to meet you. I hope I won't crash on you.\n" +
                     "I was written by @Drutol#5419 in C# using DSharpPlus library and my very own framework. You can find my insides on github!";

            builder.AddField("About", about);


            return args.Channel.SendMessageAsync(null, false, builder.Build());
        }

        [CommandHandler(Regex = @"help .*")]
        public async Task HelpCommand(MessageCreateEventArgs args, ICommandExecutionContext context)
        {
            var cmd = args.Message.Content.Substring(5 + CommandHandlerAttribute.CommandStarter.Length).Trim();
            var handler = ResourceLocator.BotContext.Commands.Values.SelectMany(list => list).FirstOrDefault(
                attribute => attribute.HumanReadableCommand != null &&
                             attribute.HumanReadableCommand.Substring(attribute.IgnoreRegexWrap ? 0 :CommandHandlerAttribute.CommandStarter.Length)
                             .ToLower().Equals(cmd.ToLower()));

            if (handler == null)
                handler = ResourceLocator.BotContext.Commands.Values.SelectMany(list => list).FirstOrDefault(
                    attribute => attribute.HumanReadableCommand != null &&
                                 attribute.HumanReadableCommand.Substring(attribute.IgnoreRegexWrap
                                         ? 0
                                         : CommandHandlerAttribute.CommandStarter.Length)
                                     .ToLower().StartsWith(cmd.ToLower()));
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
