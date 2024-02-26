using MySharpChat.Core.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace MySharpChat.Core.Model
{
    [Serializable]
    public sealed class ChatMessage : IEquatable<ChatMessage>, IEqualityComparer<ChatMessage>, IObjectWithId
    {
        public Guid Id { get; private set; }
        public User User { get; private set; }
        public DateTime Date { get; private set; }
        public string Message { get; private set; }

        [JsonConstructor]
        public ChatMessage(Guid id, User user, DateTime date, string message)
        {
            Id = id;
            User = user;
            Date = date;
            Message = message;
        }

        public static readonly IEqualityComparer<ChatMessage> Comparer = new ChatMessageEqualityComparer();
        private sealed class ChatMessageEqualityComparer : IEqualityComparer<ChatMessage>
        {
            public bool Equals(ChatMessage? x, ChatMessage? y)
            {
                return x is not null && y is not null && x.Id == y.Id;
            }

            public int GetHashCode([DisallowNull] ChatMessage obj)
            {
                return obj.Id.GetHashCode();
            }
        }

        public bool Equals(ChatMessage? other)
        {
            return other is not null && Comparer.Equals(this, other);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as ChatMessage);
        }

        public override int GetHashCode()
        {
            return Comparer.GetHashCode(this);
        }

        bool IEqualityComparer<ChatMessage>.Equals(ChatMessage? x, ChatMessage? y)
        {
            return Comparer.Equals(x, y);
        }

        int IEqualityComparer<ChatMessage>.GetHashCode([DisallowNull] ChatMessage obj)
        {
            return Comparer.GetHashCode(obj);
        }
    }
}
