using MySharpChat.Core.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.Json.Serialization;

namespace MySharpChat.Core.Model
{
    [Serializable]
    public sealed class UserState : IEquatable<UserState>, IObjectWithId
    {
        public User User { get; private set; }
        public Dictionary<DateTime, ConnexionStatus> ConnexionHistory { get; private set; } = new();

        public Guid Id => User.Id;
        public ConnexionStatus LastConnexionStatus => ConnexionHistory.MaxBy((pair) => pair.Key).Value;
        
        public UserState(User user, ConnexionStatus connexionStatus)
        {
            User = user;
            ConnexionHistory.Add(DateTime.Now, connexionStatus);
        }

        [JsonConstructor]
        public UserState(User user, Dictionary<DateTime, ConnexionStatus> connexionHistory)
        {
            User = user;
            ConnexionHistory = connexionHistory;
        }

        public bool IsConnected()
        {
            return LastConnexionStatus == ConnexionStatus.GainConnection;
        }

        public bool HasLostConnection()
        {
            return LastConnexionStatus == ConnexionStatus.LostConnection;
        }

        public void AddConnexionEvent(ConnexionStatus connexionStatus)
        {
            ConnexionHistory.Add(DateTime.Now, connexionStatus);
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
            return other != null && Comparer.Equals(this, other);
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
