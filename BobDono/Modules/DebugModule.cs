using System;
using System.Linq;
using System.Threading.Tasks;
using BobDono.Core;
using BobDono.Core.Attributes;
using BobDono.Core.Utils;
using BobDono.Interfaces;
using BobDono.Interfaces.Queries;
using BobDono.Interfaces.Services;
using DSharpPlus.EventArgs;

namespace BobDono.Modules
{
    [Module(Hidden = true,Name = "Debug",Authorize = true)]
    public class DebugModule
    {
        private readonly IExceptionHandler _exceptionHandler;
        private readonly IWaifuService _waifuService;
        private readonly ICharacterDetailsQuery _characterDetailsQuery;

        public DebugModule(IExceptionHandler exceptionHandler, IWaifuService waifuService, ICharacterDetailsQuery characterDetailsQuery)
        {
            _exceptionHandler = exceptionHandler;
            _waifuService = waifuService;
            _characterDetailsQuery = characterDetailsQuery;
        }

        [CommandHandler(Regex = @"bugs")]
        public async Task DisplayExceptions(MessageCreateEventArgs args, ICommandExecutionContext context)
        {
            if (ResourceLocator.ExceptionHandler.CaughtThings.Any())
            {
                for (int i = 0; i < Math.Min(ResourceLocator.ExceptionHandler.CaughtThings.Count,5); i++)
                {
                    var s = string.Join("\n\n",$"```{_exceptionHandler.CaughtThings[i]}```");
                    if (s.Length > 2000)
                        s = s.Substring(Math.Min(2000, s.Length));
                    await args.Channel.SendMessageAsync(s);
                }
            }
            else
            {
                await args.Channel.SendMessageAsync("No paint has been spilled as of late!");
            }
        }

        [CommandHandler(Debug = true,Regex = "updatewaifusplz")]
        public async Task UpdateWaifus(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            using (var ws = _waifuService.ObtainLifetimeHandle(executionContext))
            {
                var all = ws.GetAll();

                await args.Channel.SendMessageAsync($"Updating {all.Count} waifus.");

                foreach (var waifu in all)
                {
                    var details = await _waifuService.GetOrCreateWaifu(waifu.MalId,true);
                    waifu.Animeography = details.Animeography;
                    waifu.Mangaography = details.Mangaography;
                    waifu.Voiceactors = details.Voiceactors;
                    waifu.Description = details.Description;

                    await Task.Delay(100);
                }

                await args.Channel.SendMessageAsync("Update complete.");
            }
        }

        [CommandHandler(Regex = "proudlyproclaim .*",Authorize = true)]
        public async Task SaySomething(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            await args.Channel.SendMessageAsync(args.Message.Content.Replace("b/proudlyproclaim ", ""));
        }

        [CommandHandler(Regex = @"crash",HumanReadableCommand = "crash")]
        public async Task Crash(MessageCreateEventArgs args, ICommandExecutionContext context)
        {
            await Task.Delay(500);
            throw new Exception("Let's see what happens when I spill paint myself... for art!");
        }
    }
}
