using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BobDono.Core;
using BobDono.Core.Attributes;
using BobDono.Core.BL;
using BobDono.Core.Interfaces;
using BobDono.Core.Utils;
using BobDono.DataAccess.Database;
using BobDono.Interfaces;
using DSharpPlus;
using DSharpPlus.EventArgs;

namespace BobDono
{
    public class BobDono
    {
        private DiscordClient _client;
        private IBotBackbone _botBackbone;

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
            ResourceLocator.RegisterDependencies(_client);
            BobDatabaseContext.Initialize();
            await _client.ConnectAsync();

            _client.MessageCreated += ClientOnMessageCreated;

            await Task.Delay(1000);
            _botBackbone = ResourceLocator.BotBackbone;
            _botBackbone.Initialize();
            await Task.Delay(-1);
        }
#pragma warning disable 4014
        private async Task ClientOnMessageCreated(MessageCreateEventArgs messageCreateEventArgs)
        {
            if (messageCreateEventArgs.Author.IsBot)
                return;

            Dictionary<ModuleAttribute, HashSet<IModule>> invokedModules 
                = new Dictionary<ModuleAttribute, HashSet<IModule>>();

            foreach (var handlerEntry in _botBackbone.Handlers)
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
                                await InvokeContextual(context, handlerEntry);

                                if(!invokedModules.ContainsKey(handlerEntry.Attribute.ParentModuleAttribute))
                                    invokedModules[handlerEntry.Attribute.ParentModuleAttribute] = new HashSet<IModule>();

                                if (!invokedModules[handlerEntry.Attribute.ParentModuleAttribute].Contains(context))
                                    invokedModules[handlerEntry.Attribute.ParentModuleAttribute].Add(context);
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
                    await messageCreateEventArgs.Channel.SendMessageAsync(ResourceLocator.ExceptionHandler.Handle(e));
                }
            }

            //handle fallback commands
            try
            {
                //all handlers that are contextual fallback commands and their parent modules weren't invoked
                foreach (var handlerEntry in _botBackbone.Handlers
                    .Where(entry =>
                        entry.Attribute.FallbackCommand && 
                        entry.Attribute.ParentModuleAttribute.IsChannelContextual))
                {
                    foreach (var context in handlerEntry.Attribute.ParentModuleAttribute.Contexts.Where(module =>
                        !invokedModules.ContainsKey(handlerEntry.Attribute.ParentModuleAttribute) ||
                        !invokedModules[handlerEntry.Attribute.ParentModuleAttribute].Contains(module)))
                    {
                        if (handlerEntry.Predicates.OfType<CommandPredicates.ChannelContextFilter>().First()
                            .MeetsCriteria(handlerEntry.Attribute, messageCreateEventArgs, context))
                            await InvokeContextual(context, handlerEntry);
                    }

                }
            }
            catch (Exception e)
            {
                await messageCreateEventArgs.Channel.SendMessageAsync(ResourceLocator.ExceptionHandler.Handle(e));
            }


            async Task InvokeContextual(IModule context, HandlerEntry handlerEntry)
            {
                if (handlerEntry.Attribute.Awaitable)
                    await handlerEntry.ContextualDelegateAsync.Invoke(messageCreateEventArgs, context);
                else
                    handlerEntry.ContextualDelegateAsync.Invoke(messageCreateEventArgs, context);

            }
        }
#pragma warning restore 4014
    }
}
