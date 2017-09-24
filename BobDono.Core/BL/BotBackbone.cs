using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BobDono.Core.Attributes;
using BobDono.Core.Interfaces;
using BobDono.Core.Utils;

namespace BobDono.Core.BL
{
    public class BotBackbone : IBotBackbone
    {
        private readonly IBotContext _botContext;

        public BotBackbone(IBotContext botContext)
        {
            _botContext = botContext;
        }

        public Dictionary<Type,ModuleAttribute> Modules { get; } = new Dictionary<Type, ModuleAttribute>();
        public List<HandlerEntry> Handlers { get; } = new List<HandlerEntry>();
        private readonly Dictionary<Type, object> _moduleInstances = new Dictionary<Type, object>();

        public void Initialize()
        {
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
                Modules.Add(module.module,module.attr);
                dict[module.attr] = new List<CommandHandlerAttribute>();
                object instance = null;
                if (!module.attr.IsChannelContextual)
                    instance = module.module.GetConstructor(Type.EmptyTypes).Invoke(null);

                foreach (var method in module.module.GetMethods())
                {
                    var methodAttribute = method.GetCustomAttribute<CommandHandlerAttribute>();
                    if (methodAttribute != null)
                    {
                        dict[module.attr].Add(methodAttribute);
                        methodAttribute.ParentModuleAttribute = module.attr;
                        var handler =
                            new HandlerEntry(method.GetParameters().Select(info => info.ParameterType).ToArray())
                            {
                                Predicates =
                                {
                                    CommandPredicates.Regex
                                },
                                Attribute = methodAttribute,
                            };
                        if (module.attr.Authorize || methodAttribute.Authorize)
                        {
                            handler.Predicates.Add(CommandPredicates.Authorize);
                        }

                        if (methodAttribute.LimitToChannel != null)
                            handler.Predicates.Add(CommandPredicates.Channel);

                        if(module.attr.IsChannelContextual)
                            handler.Predicates.Add(CommandPredicates.ChannelContext);

                        if(!module.attr.IsChannelContextual)
                        {
                            handler.DelegateAsync =
                            (Delegates.CommandHandlerDelegateAsync)method.CreateDelegate(
                                typeof(Delegates.CommandHandlerDelegateAsync), instance);   
                        }
                        else
                        {
                            handler.ContextualDelegateAsync = (args, context) =>
                            {
                                var del = (Delegates.CommandHandlerDelegateAsync) method.CreateDelegate(
                                    typeof(Delegates.CommandHandlerDelegateAsync), context);
                                return del(args);
                            };
                        }
                        Handlers.Add(handler);
                    }
                }
                _moduleInstances[module.module] = instance;
            }

            _botContext.Commands = dict;
        }
    }
}
