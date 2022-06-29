using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using MySharpChat.Core.Utils;

namespace MySharpChat.Core.Command
{
    internal sealed class CommandComparer : Singleton<CommandComparer>, IEqualityComparer<ICommand>
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
            bool notNull = x != null && y != null;
            bool allNull = x == null && y == null;
#pragma warning disable CS8602 // Déréférencement d'une éventuelle référence null.
            return allNull || (notNull && nameComparer.Equals(x.Name, y.Name));
#pragma warning restore CS8602 // Déréférencement d'une éventuelle référence null.
        }

        public int GetHashCode([DisallowNull] ICommand obj)
        {
            return nameComparer.GetHashCode(obj.Name);
        }
    }
}
