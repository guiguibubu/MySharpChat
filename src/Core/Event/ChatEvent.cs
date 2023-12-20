using MySharpChat.Core.Model;
using MySharpChat.Core.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MySharpChat.Core.Event
{
    [Serializable]
    public abstract class ChatEvent : IEquatable<ChatEvent>, IObjectWithId
    {
        public Guid Id { get; }
        public DateTime Date { get; }

        protected ChatEvent()
            : this(Guid.NewGuid(), DateTime.Now)
        { }

        protected ChatEvent(Guid id, DateTime date)
        {
            Id = id;
            Date = date;
        }

        public static readonly IEqualityComparer<ChatEvent> Comparer = new ChatEventEqualityComparer();
        private sealed class ChatEventEqualityComparer : IEqualityComparer<ChatEvent>
        {
            public bool Equals(ChatEvent? x, ChatEvent? y)
            {
                return x != null && y != null && x.Id == y.Id;
            }

            public int GetHashCode([DisallowNull] ChatEvent obj)
            {
                return obj.Id.GetHashCode();
            }
        }

        public bool Equals(ChatEvent? other)
        {
            return other != null && Comparer.Equals(this, other);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as ChatEvent);
        }

        public override int GetHashCode()
        {
            return Comparer.GetHashCode(this);
        }

        public static bool operator !=(ChatEvent? x, ChatEvent? y)
        {
            return ReferenceEquals(x, null) || ReferenceEquals(y, null) || !Comparer.Equals(x, y);
        }

        public static bool operator ==(ChatEvent? x, ChatEvent? y)
        {
            return !(x != y);
        }
    }
}
