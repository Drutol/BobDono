using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BobDono.Attributes;
using BobDono.Utils;
using DSharpPlus;
using DSharpPlus.EventArgs;

namespace BobDono
{
    public class BobDono
    {
        private DiscordClient _client;
        private readonly List<HandlerEntry> _handlers = new List<HandlerEntry>();
        private readonly Dictionary<Type, object> _moduleInstances = new Dictionary<Type, object>();

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

            var assembly = Assembly.GetEntryAssembly();
            List<(ModuleAttribute attr, Type module)> modules =
                new List<(ModuleAttribute attr, Type module)>();
            foreach (var type in assembly.GetTypes())
            {
                var attr = type.GetTypeInfo().GetCustomAttribute<ModuleAttribute>();
                if (attr != null)
                    modules.Add((attr, type));
            }

            var dict = new Dictionary<ModuleAttribute, List<CommandHandlerAttribute>>();

            foreach (var module in modules)
            {
                dict[module.attr] = new List<CommandHandlerAttribute>();
                var instance = module.module.GetConstructor(Type.EmptyTypes).Invoke(null);
                foreach (var method in module.module.GetMethods())
                {
                    var methodAttribute = method.GetCustomAttribute<CommandHandlerAttribute>();
                    if (methodAttribute != null)
                    {
                        dict[module.attr].Add(methodAttribute);
                        var handler =
                            new HandlerEntry(method.GetParameters().Select(info => info.ParameterType).ToArray())
                            {
                                Predicates =
                                {
                                    CommandPredicates.Regex
                                },
                                Attribute = methodAttribute
                            };
                        if (module.attr.Authorize || methodAttribute.Authorize)
                        {
                            handler.Predicates.Add(CommandPredicates.Authorize);
                        }

                        if (methodAttribute.LimitToChannel != null)
                            handler.Predicates.Add(CommandPredicates.Channel);

                        handler.DelegateAsync =
                            (Delegates.CommandHandlerDelegateAsync) method.CreateDelegate(
                                typeof(Delegates.CommandHandlerDelegateAsync), instance);

                        _handlers.Add(handler);
                    }
                }
                _moduleInstances[module.module] = instance;

            }

            BotContext.Commands = dict;
            await Task.Delay(-1);
        }

        private async Task ClientOnMessageCreated(MessageCreateEventArgs messageCreateEventArgs)
        {
            if (messageCreateEventArgs.Author.IsBot)
                return;

            foreach (var handlerEntry in _handlers)
            {
                if (!handlerEntry.AreTypesEqual(typeof(MessageCreateEventArgs)))
                    continue;                
                try
                {
                    if (handlerEntry.Predicates.All(predicate =>
                        predicate.MeetsCriteria(handlerEntry.Attribute, messageCreateEventArgs)))
                    {
                        if (handlerEntry.Attribute.Awaitable)
                            await handlerEntry.DelegateAsync.Invoke(messageCreateEventArgs);
                        else
#pragma warning disable 4014
                            handlerEntry.DelegateAsync.Invoke(messageCreateEventArgs);
#pragma warning restore 4014
                    }
                }
                catch (Exception e)
                {
                    await messageCreateEventArgs.Channel.SendMessageAsync(BotContext.ExceptionHandler.Handle(e));
                }

            }
        }
    }
}
