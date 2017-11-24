using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BobDono.Core.Attributes;
using BobDono.Core.Interfaces;
using BobDono.Interfaces;
using DSharpPlus.EventArgs;

namespace BobDono.Modules
{
    [Module(Name = "Fluff",Description = "Clearly commands of greatest importance.")]
    class FluffModule
    {
        private readonly IBotBackbone _botBackbone;

        public FluffModule(IBotBackbone botBackbone)
        {
            _botBackbone = botBackbone;
        }

        [CommandHandler(Regex = "annak",HumanReadableCommand = "annak",HelpText = "Show annak in her full glory. Btw, it's Asuka from AoKana.")]
        public async Task Annak(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            await args.Channel.SendFileAsync($"{AppContext.BaseDirectory}/Assets/annak.png");
        }

        [CommandHandler(Regex = @":\)",HumanReadableCommand = ":)", HelpText = ":)")]
        public async Task Raphi(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            await args.Channel.SendFileAsync($"{AppContext.BaseDirectory}/Assets/raphi.png");
        }

        [CommandHandler(Regex = "vigne",HumanReadableCommand = "vigne", HelpText = "Wow...")]
        public async Task Vigne(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            await args.Channel.SendFileAsync($"{AppContext.BaseDirectory}/Assets/vigne.png");
        }

        [CommandHandler(Regex = "tasts",HumanReadableCommand = "tasts", HelpText = "Yours are quite inferior.")]
        public async Task Tasts(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            await args.Channel.SendFileAsync($"{AppContext.BaseDirectory}/Assets/tasts.png");
        }

        [CommandHandler(Regex = "rika",HumanReadableCommand = "rika", HelpText = "Could you maybe just stop?")]
        public async Task Rika(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            await args.Channel.SendFileAsync($"{AppContext.BaseDirectory}/Assets/rika.png");
        }

        [CommandHandler(Regex = "help",HumanReadableCommand = "help", HelpText = "Guides you to help?")]
        public async Task CryForHelp(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            await args.Channel.SendFileAsync($"{AppContext.BaseDirectory}/Assets/cryforhelp.png");
        }

        [CommandHandler(IgnoreRegexWrap = true, Regex = ".*java.*", HumanReadableCommand = "..java..",
            HelpText = "Oh sorry, I have allergy for **this** word.")]
        public async Task CoughCough(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            if (!(_botBackbone.ModuleInstances[typeof(ElectionsModule)] as ElectionsModule).ElectionsContexts.Any(
                context => context.ChannelIdContext == args.Channel.Id))
            {
                if(!args.Message.Content.ToLower().Contains("javascript"))
                    await args.Channel.SendMessageAsync("*cough cough*");
            }
        }
    }
}
