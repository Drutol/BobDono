using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BobDono.Core.Attributes;
using BobDono.Interfaces;
using DSharpPlus.EventArgs;

namespace BobDono.Modules
{
    [Module(Name = "Fluff",Description = "Clearly commands of greatest importance.")]
    class FluffModule
    {
        public FluffModule()
        {
            
        }

        [CommandHandler(Regex = "annak",HumanReadableCommand = "annak",HelpText = "Show annak in her full glory. Btw, it's Asuka from AoKana.")]
        public async Task Annak(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            await args.Channel.SendFileAsync($"{AppContext.BaseDirectory}/Assets/annak.png");
        }

        [CommandHandler(IgnoreRegexWrap = true,Regex = ".*java.*",HumanReadableCommand = "..java..",HelpText = "Oh sorry, I have allergy for **this** word.")]
        public async Task CoughCough(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            await args.Channel.SendMessageAsync("*cough cough*");
        }
    }
}
