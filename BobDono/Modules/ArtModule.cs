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

        [CommandHandler(IgnoreRegexWrap = true,Regex = "^b\\\\annak$", HumanReadableCommand = "b\\annak", HelpText = ".anaKoA morf akusA s'ti ,wtB .yrolg lluf reh ni kanna wohS")]
        public async Task ReverseAnnak(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            await args.Channel.SendFileAsync($"{AppContext.BaseDirectory}/Assets/akusa.png");
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

        [CommandHandler(Regex = "onegai", HumanReadableCommand = "onegai", HelpText = "You **can't** resist...")]
        public async Task Onegai(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            await args.Channel.SendFileAsync($"{AppContext.BaseDirectory}/Assets/onegai.png");
        }

        [CommandHandler(Regex = "maturity", HumanReadableCommand = "maturity", HelpText = "Macfags...")]
        public async Task Maturity(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            await args.Channel.SendFileAsync($"{AppContext.BaseDirectory}/Assets/maturity.png");
        }

        [CommandHandler(Regex = "lolicry", HumanReadableCommand = "lolicry", HelpText = "You make a loli cry, how will you make up for it?")]
        public async Task Lolicry(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            await args.Channel.SendFileAsync($"{AppContext.BaseDirectory}/Assets/lolicry.png");
        }

        [CommandHandler(Regex = "misaki", HumanReadableCommand = "misaki", HelpText = "Warm smile just for you :)")]
        public async Task Misaki(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            await args.Channel.SendFileAsync($"{AppContext.BaseDirectory}/Assets/misaki.png");
        }

        [CommandHandler(Regex = "nani", HumanReadableCommand = "nani", HelpText = "Nani the heck!?")]
        public async Task Nani(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            await args.Channel.SendFileAsync($"{AppContext.BaseDirectory}/Assets/nani.png");
        }

        [CommandHandler(Regex = "cry", HumanReadableCommand = "shrug", HelpText = "That's serious cry don't overuse it...")]
        public async Task Cry(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            if (args.Author.Id == 202501452596379648) //fox
            {
                await args.Channel.SendFileAsync($"{AppContext.BaseDirectory}/Assets/lolicry.png");
            }
            else
            {
                await args.Channel.SendFileAsync($"{AppContext.BaseDirectory}/Assets/cry.png");
            }

        }
    }
}
