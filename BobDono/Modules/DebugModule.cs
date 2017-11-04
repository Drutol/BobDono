using System;
using System.Linq;
using System.Threading.Tasks;
using BobDono.Core;
using BobDono.Core.Attributes;
using BobDono.Core.Utils;
using BobDono.Interfaces;
using DSharpPlus.EventArgs;

namespace BobDono.Modules
{
    [Module(Hidden = true,Name = "Debug",Authorize = true)]
    public class DebugModule
    {
        private readonly IExceptionHandler _exceptionHandler;

        public DebugModule(IExceptionHandler exceptionHandler)
        {
            _exceptionHandler = exceptionHandler;
        }

        [CommandHandler(Regex = @"bugs")]
        public async Task DisplayExceptions(MessageCreateEventArgs args, ICommandExecutionContext context)
        {
            if (ResourceLocator.ExceptionHandler.CaughtThings.Any())
            {
                var s = string.Join("\n\n",
                    _exceptionHandler.CaughtThings.Select(exception =>
                        $"```{exception}```"));
                if (s.Length > 2000)
                    s = s.Substring(0, 2000);
                await args.Channel.SendMessageAsync(s);
            }
            else
            {
                await args.Channel.SendMessageAsync("No paint has been spilled as of late!");
            }
        }

        [CommandHandler(Regex = @"crash",HumanReadableCommand = "crash")]
        public async Task Crash(MessageCreateEventArgs args, ICommandExecutionContext context)
        {
            await Task.Delay(500);
            throw new Exception("Let's see what happens when I spill paint myself... for art!");
        }
    }
}
