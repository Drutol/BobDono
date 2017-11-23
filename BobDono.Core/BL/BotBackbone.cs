using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using BobDono.Core.Attributes;
using BobDono.Core.Interfaces;
using BobDono.Core.Utils;
using BobDono.Interfaces.Services;

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
        public Dictionary<Type, object> ModuleInstances { get; } = new Dictionary<Type, object>();

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

            using (var dependencyScope = ResourceLocator.ObtainScope())
            {
                foreach (var module in modules.OrderByDescending(tuple => tuple.attr.IsChannelContextual))
                {
                    Modules.Add(module.module, module.attr);
                    dict[module.attr] = new List<CommandHandlerAttribute>();
                    object instance = null;
                    if (!module.attr.IsChannelContextual)
                        instance = dependencyScope.Resolve(module.module);

                    foreach (var method in module.module.GetMethods())
                    {
                        var methodAttribute = method.GetCustomAttribute<CommandHandlerAttribute>();
                        if (methodAttribute != null)
                        {
                            dict[module.attr].Add(methodAttribute);
                            methodAttribute.ParentModuleAttribute = module.attr;
                            var handler =
                                new HandlerEntry(methodAttribute,
                                    method.GetParameters().Select(info => info.ParameterType).ToArray());

                            if(!string.IsNullOrEmpty(methodAttribute.Regex))
                                handler.Predicates.Add(CommandPredicates.Regex);

                            if (module.attr.Authorize || methodAttribute.Authorize || methodAttribute.Debug)
                                handler.Predicates.Add(CommandPredicates.Authorize);

                            if (methodAttribute.LimitToChannel != null)
                                handler.Predicates.Add(CommandPredicates.Channel);

                            if (module.attr.IsChannelContextual)
                                handler.Predicates.Add(CommandPredicates.ChannelContext);

                            if(methodAttribute.FallbackCommand)
                                handler.Predicates.Insert(0,CommandPredicates.AlwaysFail);

                            if (!module.attr.IsChannelContextual)
                            {
                                handler.DelegateAsync =
                                    (Delegates.CommandHandlerDelegateAsync) method.CreateDelegate(
                                        typeof(Delegates.CommandHandlerDelegateAsync), instance);
                            }
                            else
                            {
                                handler.ContextualDelegateAsync = (args, context, executionContext) =>
                                {
                                    var del = (Delegates.CommandHandlerDelegateAsync) method.CreateDelegate(
                                        typeof(Delegates.CommandHandlerDelegateAsync), context);
                                    return del(args,executionContext);
                                };
                            }
                            Handlers.Add(handler);
                        }
                    }
                    ModuleInstances[module.module] = instance;
                }
            }
            _botContext.Commands = dict;
        }

        public static IEnumerable<Type> GetModules()
        {
            var assembly = Assembly.GetEntryAssembly();
            List<(ModuleAttribute attr, Type module)> modules =
                new List<(ModuleAttribute attr, Type module)>();
            foreach (var type in assembly.GetTypes())
            {
                var attr = type.GetTypeInfo().GetCustomAttribute<ModuleAttribute>();
                if (attr != null)
                    yield return type;
            }
        }
    }
}
