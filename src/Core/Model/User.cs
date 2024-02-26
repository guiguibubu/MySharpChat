using MySharpChat.Core.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace MySharpChat.Core.Model
{
    [Serializable]
    public sealed class User : IEquatable<User>, IObjectWithId
    {
        public Guid Id { get; private set; }
        public string Username { get; set; }

        [JsonConstructor]
        public User(Guid id, string username)
        {
            Id = id;
            Username = username;
        }
        
        public override string ToString()
        {
            return string.Format("{0} (id : {1})", Username, Id);
        }

        public static readonly IEqualityComparer<User> Comparer = new UserEqualityComparer();
        private sealed class UserEqualityComparer : IEqualityComparer<User>
        {
            public bool Equals(User? x, User? y)
            {
                return x is not null && y is not null && x.Id == y.Id;
            }

            public int GetHashCode([DisallowNull] User obj)
            {
                return obj.Id.GetHashCode();
            }
        }

        public bool Equals(User? other)
        {
            return other is not null && Comparer.Equals(this, other);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as User);
        }

        public override int GetHashCode()
        {
            return Comparer.GetHashCode(this);
        }

        public static bool operator !=(User? x, User? y)
        {
            return ReferenceEquals(x, null) || ReferenceEquals(y, null) || !Comparer.Equals(x, y);
        }

        public static bool operator ==(User? x, User? y)
        {
            return !(x != y);
        }
    }
}
