using System;
using System.Linq;
using System.Threading.Tasks;
using BobDono.Attributes;
using BobDono.Utils;
using DSharpPlus.EventArgs;

namespace BobDono.Modules
{
    [Module(Hidden = true,Name = "Debug",Authorize = true)]
    public class DebugModule
    {
        [CommandHandler(Regex = @"bugs")]
        public async Task DisplayExceptions(MessageCreateEventArgs args)
        {
            if (BotContext.ExceptionHandler.CaughtThings.Any())
            {
                await args.Channel.SendMessageAsync(string.Join("\n\n",
                    BotContext.ExceptionHandler.CaughtThings.Select(exception =>
                        $"```{exception}```")).Substring(0,2000));
            }
            else
            {
                await args.Channel.SendMessageAsync("No paint has been spilled as of late!");
            }
        }

        [CommandHandler(Regex = @"crash",HumanReadableCommand = "crash")]
        public async Task Crash(MessageCreateEventArgs args)
        {
            await Task.Delay(500);
            throw new Exception("Let's see what happens when I spill paint myself... for art!");
        }
    }
}
