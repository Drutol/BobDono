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

        public static async Task Main(string[] args)
        {
            //BracketImageGenerator.Generate(new List<byte[]>
            //{
            //    File.ReadAllBytes("Assets/annak.png") ,
            //    File.ReadAllBytes("Assets/vigne.png") ,
            //    File.ReadAllBytes("Assets/raphi.png") ,
                
            //});
            //GenuineCertificatesGenerator.Generate(new List<string>{"Holo is life","Holo is life"});
            //return;
            var prog = new BobDono();
            await prog.RunBotAsync();
        }

        private async Task RunBotAsync()
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-GB");
            _client = new CustomDiscordClient(new DiscordConfiguration
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



            await _client.UpdateStatusAsync(new DiscordGame("b/help for no help"), UserStatus.Online);
            _client.Resumed += async args =>
            {
                await _client.UpdateStatusAsync(new DiscordGame("b/help for no help"), UserStatus.Online);
            };
         
            if (_client.Guilds.Count > 2)
            {
                foreach (var guild in _client.Guilds)
                {
                    if (guild.Key != 317924870950223872 && guild.Key != 343060137164144642)
                        await guild.Value.LeaveAsync();
                }
            }

            await Task.Delay(-1);
        }
#pragma warning disable 4014
        private async Task ClientOnMessageCreated(MessageCreateEventArgs messageCreateEventArgs)
        {

            //Console.WriteLine(messageCreateEventArgs.Message.Content);
            if (messageCreateEventArgs.Author.IsBot)
            {
                if (!messageCreateEventArgs.Author.IsMe() && ContextModuleBase.ContextChannels.Any(arg => arg == messageCreateEventArgs.Channel.Id))
                {
                    await messageCreateEventArgs.Message.DeleteAsync();
                }
                return;
            }

            Dictionary<ModuleAttribute, HashSet<IModule>> invokedModules 
                = new Dictionary<ModuleAttribute, HashSet<IModule>>();

            foreach (var handlerEntry in _botBackbone.Handlers)
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

                                if(!invokedModules.ContainsKey(handlerEntry.Attribute.ParentModuleAttribute))
                                    invokedModules[handlerEntry.Attribute.ParentModuleAttribute] = new HashSet<IModule>();

                                if (!invokedModules[handlerEntry.Attribute.ParentModuleAttribute].Contains(context))
                                    invokedModules[handlerEntry.Attribute.ParentModuleAttribute].Add(context);
                            }
                        }
                    }
                    else
                    {
                        //TODO
                        if(ResourceLocator.BotBackbone.Modules[typeof(ElectionContext)].Contexts.Any(module => module.ChannelIdContext == messageCreateEventArgs.Channel.Id))
                            continue;
                        if (ResourceLocator.BotBackbone.Modules[typeof(ElectionThemesContext)].Contexts.Any(module => module.ChannelIdContext == messageCreateEventArgs.Channel.Id))
                            continue;
                        if (ResourceLocator.BotBackbone.Modules[typeof(HallOfFameContext)].Contexts.Any(module => module.ChannelIdContext == messageCreateEventArgs.Channel.Id))
                            continue;
                        if (ResourceLocator.BotBackbone.Modules[typeof(MatchupContext)].Contexts.Any(module => module.ChannelIdContext == messageCreateEventArgs.Channel.Id))
                            continue;

                        if (handlerEntry.Predicates.All(predicate =>
                            predicate.MeetsCriteria(handlerEntry.Attribute, messageCreateEventArgs)))
                        {
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
