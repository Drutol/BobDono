using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BobDono.Attributes;
using BobDono.Interfaces;
using DSharpPlus.EventArgs;

namespace BobDono.Utils
{
    public class HandlerEntry
    {
        class TypeComaprer : IEqualityComparer<Type>
        {
            public bool Equals(Type x, Type y) => x.Equals(y);

            public int GetHashCode(Type obj) => obj.GetHashCode();
        }

        private static readonly TypeComaprer _typeComaprer = new TypeComaprer();

        private readonly Type[] _arguments;

        public HandlerEntry(Type[] arguments)
        {
            _arguments = arguments;
        }

        public CommandHandlerAttribute Attribute { get; set; }

        public List<ICommandPredicate> Predicates { get; } =
            new List<ICommandPredicate>();

        public Delegates.CommandHandlerDelegate Executor { get; set; }

        public bool AreTypesEqual(params Type[] types)
        {
            return _arguments.SequenceEqual(types,_typeComaprer);
        }
    }
}
