using MySharpChat.Core.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace MySharpChat.Core.Event
{
    [Serializable]
    [JsonDerivedType(typeof(ChatMessageEvent))]
    [JsonDerivedType(typeof(ConnexionEvent))]
    [JsonDerivedType(typeof(DisconnexionEvent))]
    [JsonDerivedType(typeof(UsernameChangeEvent))]
    public abstract class ChatEvent : IEqualityComparer<ChatEvent>, IObjectWithId
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

        public override bool Equals(object? obj)
        {
            return ((IEqualityComparer<ChatEvent>)this).Equals(this, obj as ChatEvent);
        }

        public override int GetHashCode()
        {
            return Comparer.GetHashCode(this);
        }

        bool IEqualityComparer<ChatEvent>.Equals(ChatEvent? x, ChatEvent? y)
        {
            return Comparer.Equals(x, y);
        }

        int IEqualityComparer<ChatEvent>.GetHashCode([DisallowNull] ChatEvent obj)
        {
            return Comparer.GetHashCode(obj);
        }
    }
}
