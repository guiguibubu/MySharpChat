using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using MySharpChat.Core.Utils;

namespace MySharpChat.Core.Command
{
    internal class CommandComparer : Singleton<CommandComparer>, IEqualityComparer<ICommand>
    {
        private static readonly StringComparer nameComparer = StringComparer.InvariantCultureIgnoreCase;
        protected CommandComparer() { }

        public static StringComparer NameComparer
        {
            get
            {
                return nameComparer;
            }
        }

        public bool Equals(ICommand? x, ICommand? y)
        {
            bool notNull = x is not null && y is not null;
            bool allNull = x is null && y is null;
            return allNull || (notNull && nameComparer.Equals(x!.Name, y!.Name));
        }

        public int GetHashCode([DisallowNull] ICommand obj)
        {
            return nameComparer.GetHashCode(obj.Name);
        }
    }
}
