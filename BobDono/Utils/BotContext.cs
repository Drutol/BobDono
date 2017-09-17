using System.Collections.Generic;
using System.Threading.Tasks;
using BobDono.Attributes;
using BobDono.BL;
using BobDono.Database;
using BobDono.Interfaces;
using BobDono.MalHell.Comm;
using BobDono.MalHell.Queries;
using DSharpPlus;
using Microsoft.EntityFrameworkCore;

namespace BobDono.Utils
{
    /// <summary>
    /// I don't want to bother if IoC and such so... /shrug
    /// </summary>
    public static class BotContext
    {
        private static DiscordClient _discordClient;
        public const string CommandStarter = "!";

        public static IHttpClientProvider HttpClientProvider { get; } 
        public static CharacterDetailsQuery CharacterDetailsQuery { get; }
        public static ProfileQuery ProfileQuery { get; }
        public static StaffDetailsQuery StaffDetailsQuery { get; }
        public static CharactersSearchQuery CharactersSearchQuery { get; }
        public static Dictionary<ModuleAttribute,List<CommandHandlerAttribute>> Commands { get; set; }
        public static ExceptionHandler ExceptionHandler { get; }

        public static DiscordClient DiscordClient
        {
            get { return _discordClient; }
            set
            {
                _discordClient = value;

                value.MessageCreated += args =>
                {
                    if (args.Channel.IsPrivate)
                        NewPrivateMessage?.Invoke(args);
                    return Task.CompletedTask;
                };
            }
        }

        public static event Delegates.CommandHandlerDelegate NewPrivateMessage;
        

        static BotContext()
        {
            HttpClientProvider = new HttpClientProvider();
            CharacterDetailsQuery = new CharacterDetailsQuery(HttpClientProvider);
            ProfileQuery = new ProfileQuery(HttpClientProvider);
            StaffDetailsQuery = new StaffDetailsQuery(HttpClientProvider);
            CharactersSearchQuery = new CharactersSearchQuery(HttpClientProvider);
            ExceptionHandler = new ExceptionHandler();
        }
    }
}
