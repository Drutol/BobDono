using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BobDono.Core.Attributes;
using BobDono.Core.Utils;

namespace BobDono.Core.BL
{
    [DebuggerDisplay("{" + nameof(Regex) + "}")]
    public class HandlerEntry
    {
        class TypeComaprer : IEqualityComparer<Type>
        {
            public bool Equals(Type x, Type y) => x.Equals(y);

            public int GetHashCode(Type obj) => obj.GetHashCode();
        }

        private static readonly TypeComaprer _typeComaprer = new TypeComaprer();

        private readonly Type[] _arguments;

        public HandlerEntry(CommandHandlerAttribute attribute, Type[] arguments)
        {
            Attribute = attribute;
            _arguments = arguments;
        }

        public CommandHandlerAttribute Attribute { get; }

        public List<ICommandPredicate> Predicates { get; } =
            new List<ICommandPredicate>();

        private string Regex => Attribute.Regex;

        public Delegates.CommandHandlerDelegateAsync DelegateAsync { get; set; }
        public Delegates.ContextualCommandHandlerDelegateAsync ContextualDelegateAsync { get; set; }


        public bool AreTypesEqual(params Type[] types)
        {
            return _arguments.SequenceEqual(types,_typeComaprer);
        }
    }
}
