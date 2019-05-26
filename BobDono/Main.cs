using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BobDono.Contexts;
using BobDono.Controllers;
using BobDono.Core;
using BobDono.Core.Attributes;
using BobDono.Core.BL;
using BobDono.Core.Extensions;
using BobDono.Core.Interfaces;
using BobDono.Core.Utils;
using BobDono.DataAccess.Database;
using BobDono.Interfaces;
using BobDono.Models;
using BobDono.Models.Entities.Stats;
using BobDono.Utils;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace BobDono
{
    public class BobDono
    {
        private DiscordClient _client;
        private IBotBackbone _botBackbone;
        private Stopwatch _stopwatch = new Stopwatch();
        private bool _ready;

        public static async Task Main(string[] args)
        {
            Console.WriteLine("BootingBob!");
            try
            {
                var prog = new BobDono();
                await prog.RunBotAsync();
            }
            catch (Exception e)
            {
                Debugger.Break();
            }

        }

        private async Task RunBotAsync()
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-GB");
            _client = new CustomDiscordClient(new DiscordConfiguration
            {
                Token = Config.BotKey,
                TokenType = TokenType.Bot,

                AutoReconnect = true,
#if DEBUG
                LogLevel = LogLevel.Debug,
#else
                LogLevel = LogLevel.Info,
#endif
                UseInternalLogHandler = true
            });



            Console.WriteLine("Starting dependency registration.");
            _stopwatch.Start();
            ResourceLocator.RegisterDependencies(_client);
            _stopwatch.Stop();
            Console.WriteLine($"Finished dependency registration, took {_stopwatch.ElapsedMilliseconds}ms.");


            Console.WriteLine("Connecting to Discord.");
            _stopwatch.Restart();
            await _client.ConnectAsync();
            _stopwatch.Stop();
            Console.WriteLine($"Connected, took {_stopwatch.ElapsedMilliseconds}ms.");


            _client.MessageCreated += ClientOnMessageCreated;

            await Task.Delay(1000);

            _botBackbone = ResourceLocator.BotBackbone;
            Console.WriteLine("Initializing framework.");
            _stopwatch.Restart();
            _botBackbone.Initialize();
            _stopwatch.Stop();
            Console.WriteLine($"Finished framework initialization, took {_stopwatch.ElapsedMilliseconds}ms.");


            await _client.UpdateStatusAsync(new DiscordGame("b/help for no help"), UserStatus.Online);
            _client.Resumed += async args =>
            {
                await _client.UpdateStatusAsync(new DiscordGame("b/help for no help"), UserStatus.Online);
            };
         
            if (_client.Guilds.Count >= 2)
            {
                foreach (var guild in _client.Guilds)
                {
                    if (!Config.ServerIds.Contains(guild.Key))
                        await guild.Value.LeaveAsync();
                }
            }

            _ready = true;
            await Task.Delay(-1);
        }
#pragma warning disable 4014
        private async Task ClientOnMessageCreated(MessageCreateEventArgs messageCreateEventArgs)
        {
            if(!_ready)
                return;

            HandlerEntry invokedhandler = null;
            //Console.WriteLine(messageCreateEventArgs.Message.Content);
            if (messageCreateEventArgs.Author.IsBot)
            {
                if (!messageCreateEventArgs.Author.IsMe() &&
                    ContextModuleBase.ContextChannels.Any(arg => arg == messageCreateEventArgs.Channel.Id))
                {
                    await messageCreateEventArgs.Message.DeleteAsync();
                    return;
                }
            }

            Dictionary<ModuleAttribute, HashSet<IModule>> invokedModules 
                = new Dictionary<ModuleAttribute, HashSet<IModule>>();

            List<HandlerEntry> entries;

            if (messageCreateEventArgs.Author.IsBot)
                entries = _botBackbone.Handlers.Where(entry => entry.Attribute.AcceptsBotCalls).ToList();
            else
                entries = _botBackbone.Handlers;

            foreach (var handlerEntry in entries)
            {
                if (!handlerEntry.AreTypesEqual(typeof(MessageCreateEventArgs),typeof(ICommandExecutionContext)))
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
                                var ctx = ResourceLocator.ExecutionContext;
                                ctx.AuthenticatedCaller = messageCreateEventArgs.Author.IsAuthenticated();
                                await InvokeContextual(context, handlerEntry, ctx);

                                invokedhandler = handlerEntry;

                                if(!invokedModules.ContainsKey(handlerEntry.Attribute.ParentModuleAttribute))
                                    invokedModules[handlerEntry.Attribute.ParentModuleAttribute] = new HashSet<IModule>();

                                if (!invokedModules[handlerEntry.Attribute.ParentModuleAttribute].Contains(context))
                                    invokedModules[handlerEntry.Attribute.ParentModuleAttribute].Add(context);
                            }
                        }
                    }
                    else
                    {
                        if (!handlerEntry.Attribute.AllowInContextChannels)
                        {
                            if (ResourceLocator.BotBackbone.Modules.Any(pair =>
                                pair.Value.Contexts.Any(module =>
                                    module.ChannelIdContext == messageCreateEventArgs.Channel.Id)))
                                continue;
                        }


                        if (handlerEntry.Predicates.All(predicate =>
                            predicate.MeetsCriteria(handlerEntry.Attribute, messageCreateEventArgs)))
                        {
                            invokedhandler = handlerEntry;

                            var ctx = ResourceLocator.ExecutionContext;
                            ctx.AuthenticatedCaller = messageCreateEventArgs.Author.IsAuthenticated();
                            if (handlerEntry.Attribute.Awaitable)
                                await handlerEntry.DelegateAsync.Invoke(messageCreateEventArgs, ctx);
                            else
                                handlerEntry.DelegateAsync.Invoke(messageCreateEventArgs, ctx);
                        }
                    }
                }
                catch (Exception e)
                {
                    var res = ResourceLocator.ExceptionHandler.Handle(e,messageCreateEventArgs);
                    if (!handlerEntry.Attribute.ParentModuleAttribute.IsChannelContextual)
                        await messageCreateEventArgs.Channel.SendMessageAsync(res);
                }
            }

            //handle fallback commands
            try
            {
                //all handlers that are contextual fallback commands and their parent modules weren't invoked
                foreach (var handlerEntry in entries
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
                        {
                            var ctx = ResourceLocator.ExecutionContext;
                            ctx.AuthenticatedCaller = messageCreateEventArgs.Author.IsAuthenticated();
                            await InvokeContextual(context, handlerEntry , ctx);
                        }
                    }

                }
            }
            catch (Exception e)
            {
                ResourceLocator.ExceptionHandler.Handle(e,messageCreateEventArgs);
            }

            if (invokedhandler != null)
            {
                Messenger.Instance.Send(new ExecutedCommand()
                {
                    CallerName = messageCreateEventArgs.Author.Username,
                    CallerHash = messageCreateEventArgs.Author.Username.GetHashCode(),

                    CommandName = invokedhandler.Attribute.HumanReadableCommand,
                    CommandHash = invokedhandler.Attribute.HandlerMethodName.GetHashCode(),

                    Contextual = invokedhandler.Attribute.ParentModuleAttribute.IsChannelContextual,
                    Existed = true,
                    Time = DateTime.UtcNow
                });
            }
            else
            {
                if (messageCreateEventArgs.Message.Content.StartsWith(CommandHandlerAttribute.CommandStarter,
                        StringComparison.InvariantCultureIgnoreCase) &&
                    messageCreateEventArgs.Message.Content.Length < 100)
                {
                    Messenger.Instance.Send(new ExecutedCommand()
                    {
                        CallerName = messageCreateEventArgs.Author.Username,
                        CommandName = messageCreateEventArgs.Message.Content.ToLower(),

                        Existed = false,
                        Time = DateTime.UtcNow
                    });
                }
            }


            async Task InvokeContextual(IModule context, HandlerEntry handlerEntry, ICommandExecutionContext executionContext)
            {
                if (handlerEntry.Attribute.Awaitable)
                    await handlerEntry.ContextualDelegateAsync.Invoke(messageCreateEventArgs, context, executionContext);
                else
                    handlerEntry.ContextualDelegateAsync.Invoke(messageCreateEventArgs, context, executionContext);
            }
        }
#pragma warning restore 4014
    }
}
