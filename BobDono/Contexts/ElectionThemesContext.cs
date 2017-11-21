using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BobDono.Core.Attributes;
using BobDono.Core.Extensions;
using BobDono.DataAccess.Services;
using BobDono.Interfaces;
using BobDono.Interfaces.Services;
using BobDono.Models.Entities.Simple;
using DSharpPlus.EventArgs;

namespace BobDono.Contexts
{
    //[Module(Name = "ElectionThemes(Channel)",Description = "Allows to submit theme ideas for elections",IsChannelContextual = true)]
    public class ElectionThemesContext : ContextModuleBase
    {
        private readonly IUserService _userService;
        private readonly IElectionThemeService _electionThemeService;
        public sealed override ulong? ChannelIdContext { get; protected set; }

        public ElectionThemesContext(ElectionThemeChannel channel,IUserService userService, IElectionThemeService electionThemeService)
        {
            _userService = userService;
            _electionThemeService = electionThemeService;
            ChannelIdContext = (ulong)channel.DiscordChannelId;
        }

        [CommandHandler(Regex = "add theme .*\n.*",HumanReadableCommand = "add theme <name>\\n<description>")]
        public async Task CreateNewElectionTheme(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            using (var userService = _userService.ObtainLifetimeHandle(executionContext))
            using (var themeService = _electionThemeService.ObtainLifetimeHandle(executionContext))
            {
                var user = userService.GetOrCreateUser(args.Author);

            }
        }

        [CommandHandler(FallbackCommand = true)]
        public async Task FallbackCommand(MessageCreateEventArgs args, ICommandExecutionContext executionContext)
        {
            if (!args.Author.IsMe())
                await args.Message.DeleteAsync();
        }
    }
}
