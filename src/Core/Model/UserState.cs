using MySharpChat.Core.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace MySharpChat.Core.Model
{
    public sealed class UserState : IEquatable<UserState>, IObjectWithId
    {
        public User User { get; private set; }
        public bool Connected { get; set; }

        public Guid Id => User.Id;

        public UserState(User user, bool connected)
        {
            User = user;
            Connected = connected;
        }

        public static readonly IEqualityComparer<UserState> Comparer = new UserStateEqualityComparer();
        private sealed class UserStateEqualityComparer : IEqualityComparer<UserState>
        {
            public bool Equals(UserState? x, UserState? y)
            {
                return x != null && y != null && x.User == y.User;
            }

            public int GetHashCode([DisallowNull] UserState obj)
            {
                return obj.User.Id.GetHashCode();
            }
        }

        public bool Equals(UserState? other)
        {
            return other!= null && Comparer.Equals(this, other);
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as UserState);
        }

        public override int GetHashCode()
        {
            return Comparer.GetHashCode(this);
        }

        public static bool operator !=(UserState? x, UserState? y)
        {
            return ReferenceEquals(x, null) || ReferenceEquals(y, null) || !Comparer.Equals(x, y);
        }

        public static bool operator ==(UserState? x, UserState? y)
        {
            return !(x != y);
        }
    }
}
