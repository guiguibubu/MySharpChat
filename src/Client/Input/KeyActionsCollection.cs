using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MySharpChat.Client.Input
{
    internal class KeyActionsCollection
    {
        private readonly Dictionary<ConsoleKeyIdentifier, KeyActionDelegate> keyActions = new Dictionary<ConsoleKeyIdentifier, KeyActionDelegate>(ConsoleKeyIdentifier.Comparer);

        public void Add(ConsoleKey key, ConsoleModifiers modifiers, KeyActionDelegate action)
        {
            Add(new ConsoleKeyIdentifier(key, modifiers), action);
        }

        public void Add(ConsoleKey key, KeyActionDelegate action)
        {
            Add(new ConsoleKeyIdentifier(key), action);
        }

        private void Add(ConsoleKeyIdentifier keyIdentifier, KeyActionDelegate action)
        {
            if (keyActions.ContainsKey(keyIdentifier))
                keyActions[keyIdentifier] = action;
            else
                keyActions.Add(keyIdentifier, action);
        }

        public bool TryGetValue(ConsoleKey key, out KeyActionDelegate? action)
        {
            return keyActions.TryGetValue(new ConsoleKeyIdentifier(key), out action);
        }

        public bool TryGetValue(ConsoleKey key, ConsoleModifiers modifiers, out KeyActionDelegate? action)
        {
            return keyActions.TryGetValue(new ConsoleKeyIdentifier(key, modifiers), out action);
        }

        public bool TryGetValue(ConsoleKeyInfo key, out KeyActionDelegate? action)
        {
            return TryGetValue(key.Key, key.Modifiers, out action);
        }

        private struct ConsoleKeyIdentifier : IEquatable<ConsoleKeyIdentifier>
        {
            public readonly static ConsoleKeyIdentifierEqualityComparer Comparer = new ConsoleKeyIdentifierEqualityComparer();

            public ConsoleKeyIdentifier(ConsoleKey key, ConsoleModifiers modifiers)
            {
                Key = key;
                Modifiers = modifiers;
            }

            public ConsoleKeyIdentifier(ConsoleKey consoleKey)
            : this(consoleKey, 0)
            { }

            public ConsoleKeyIdentifier(ConsoleKeyInfo consoleKeyInfo)
                : this(consoleKeyInfo.Key, consoleKeyInfo.Modifiers)
            { }

            public ConsoleKey Key { get; }
            public ConsoleModifiers Modifiers { get; }

            public bool Equals(ConsoleKeyIdentifier other)
            {
                return Comparer.Equals(this, other);
            }
        }

        private sealed class ConsoleKeyIdentifierEqualityComparer : EqualityComparer<ConsoleKeyIdentifier>
        {
            public override bool Equals(ConsoleKeyIdentifier x, ConsoleKeyIdentifier y)
            {
                bool keyEqual = x.Key == y.Key;
                bool modifierEqual = x.Modifiers == y.Modifiers;
                return keyEqual && modifierEqual;
            }

            public override int GetHashCode([DisallowNull] ConsoleKeyIdentifier obj)
            {
                return HashCode.Combine(obj.Key, obj.Modifiers);
            }
        }
    }
}
