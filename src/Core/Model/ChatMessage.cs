using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MySharpChat.Core.Utils;

namespace MySharpChat.Core.Model
{
    public sealed class ChatMessage : IEquatable<ChatMessage>, IObjectWithId
    {
        public Guid Id { get; private set; }
        public User User { get; private set; }
        public DateTime Date { get; private set; }
        public string Message { get; private set; }

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
                return x != null && y != null && x.Id == y.Id;
            }

            public int GetHashCode([DisallowNull] ChatMessage obj)
            {
                return obj.Id.GetHashCode();
            }
        }

        public bool Equals(ChatMessage? other)
        {
            return other != null && Comparer.Equals(this, other);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as ChatMessage);
        }

        public override int GetHashCode()
        {
            return Comparer.GetHashCode(this);
        }

        public static bool operator !=(ChatMessage? x, ChatMessage? y)
        {
            return ReferenceEquals(x, null) || ReferenceEquals(y, null) || !Comparer.Equals(x, y);
        }

        public static bool operator ==(ChatMessage? x, ChatMessage? y)
        {
            return !(x != y);
        }
    }
}
