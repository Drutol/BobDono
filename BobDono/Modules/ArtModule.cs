using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BobDono.Core.Attributes;
using BobDono.Interfaces;
using DSharpPlus.EventArgs;

namespace BobDono.Modules
{
    [Module(Name = "Art",Description = "Shhh... art is here.")]
    public class ArtModule
    {

        [CommandHandler(Regex = "annak", HumanReadableCommand = "annak", HelpText = "Show annak in her full glory. Btw, it's Asuka from AoKana.")]
        public async Task Annak(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            await args.Channel.SendFileAsync($"{AppContext.BaseDirectory}/Assets/annak.png");
        }

        [CommandHandler(Regex = @":\)", HumanReadableCommand = ":)", HelpText = ":)")]
        public async Task Raphi(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            await args.Channel.SendFileAsync($"{AppContext.BaseDirectory}/Assets/raphi.png");
        }

        [CommandHandler(Regex = "vigne", HumanReadableCommand = "vigne", HelpText = "Wow...")]
        public async Task Vigne(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            await args.Channel.SendFileAsync($"{AppContext.BaseDirectory}/Assets/vigne.png");
        }

        [CommandHandler(Regex = "tasts", HumanReadableCommand = "tasts", HelpText = "Yours are quite inferior.")]
        public async Task Tasts(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            await args.Channel.SendFileAsync($"{AppContext.BaseDirectory}/Assets/tasts.png");
        }

        [CommandHandler(Regex = "rika", HumanReadableCommand = "rika", HelpText = "Could you maybe just stop?")]
        public async Task Rika(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            await args.Channel.SendFileAsync($"{AppContext.BaseDirectory}/Assets/rika.png");
        }

        [CommandHandler(Regex = "shrug", HumanReadableCommand = "shrug", HelpText = "Shruggityshrug...")]
        public async Task Shrug(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            await args.Channel.SendFileAsync($"{AppContext.BaseDirectory}/Assets/shrug.png");
        }
    }
}
