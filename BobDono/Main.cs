using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BobDono.Attributes;
using BobDono.BL;
using BobDono.Interfaces;
using BobDono.Utils;
using DSharpPlus;
using DSharpPlus.EventArgs;

namespace BobDono
{
    public class BobDono
    {
        private DiscordClient _client;
        
        public static async Task Main(string[] args)
        {
            var prog = new BobDono();
            await prog.RunBotAsync();
        }

        private async Task RunBotAsync()
        {

            _client = new DiscordClient(new DiscordConfiguration
            {
                Token = Secrets.BotKey,
                TokenType = TokenType.Bot,

                AutoReconnect = true,
                LogLevel = LogLevel.Debug,
                UseInternalLogHandler = true
            });

            await _client.ConnectAsync();

            _client.MessageCreated += ClientOnMessageCreated;

            BotContext.DiscordClient = _client;
            await Task.Delay(1000);
            BotBackbone.Instance.Initialize();

            await Task.Delay(-1);
        }
#pragma warning disable 4014
        private async Task ClientOnMessageCreated(MessageCreateEventArgs messageCreateEventArgs)
        {
            if (messageCreateEventArgs.Author.IsBot)
                return;

            foreach (var handlerEntry in BotBackbone.Instance.Handlers)
            {
                if (!handlerEntry.AreTypesEqual(typeof(MessageCreateEventArgs)))
                    continue;                
                try
                {
                    if (handlerEntry.Attribute.ParentModuleAttribute.IsChannelContextual)
                    {
                        foreach (var context in handlerEntry.Attribute.ParentModuleAttribute.Contexts)
                        {
                            if (handlerEntry.Predicates.All(predicate =>
                                predicate.MeetsCriteria(handlerEntry.Attribute, messageCreateEventArgs, context)))
                            {
                                if (handlerEntry.Attribute.Awaitable)
                                    await handlerEntry.ContextualDelegateAsync.Invoke(messageCreateEventArgs,context);
                                else

                                    handlerEntry.ContextualDelegateAsync.Invoke(messageCreateEventArgs,context);
                            }
                        
                        }
                    }
                    else
                    {
                        if (handlerEntry.Predicates.All(predicate =>
                            predicate.MeetsCriteria(handlerEntry.Attribute, messageCreateEventArgs)))
                        {
                            if (handlerEntry.Attribute.Awaitable)
                                await handlerEntry.DelegateAsync.Invoke(messageCreateEventArgs);
                            else
                                handlerEntry.DelegateAsync.Invoke(messageCreateEventArgs);
                        }
                    }

                }
                catch (Exception e)
                {
                    await messageCreateEventArgs.Channel.SendMessageAsync(BotContext.ExceptionHandler.Handle(e));
                }
            }
        }
#pragma warning restore 4014
    }
}
