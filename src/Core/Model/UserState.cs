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
        public ConnexionStatus ConnexionStatus { get; private set; }
        
        [JsonIgnore]
        public Guid Id => User.Id;

        private readonly Dictionary<DateTime, ConnexionStatus> _connexionHistory = new();

        public UserState(User user)
        {
            User = user;
        }

        [JsonConstructor]
        public UserState(User user, ConnexionStatus connexionStatus)
        {
            User = user;
            _connexionHistory.Add(DateTime.Now, connexionStatus);
            UpdateCurrentConnexionStatus();
        }

        public UserState(User user, IEnumerable<KeyValuePair<DateTime, ConnexionStatus>> connexionHistory)
        {
            User = user;
            _connexionHistory = new Dictionary<DateTime, ConnexionStatus>(connexionHistory);
            UpdateCurrentConnexionStatus();
        }

        public bool IsConnected()
        {
            return ConnexionStatus == ConnexionStatus.GainConnection;
        }

        public bool HasLostConnection()
        {
            return ConnexionStatus == ConnexionStatus.LostConnection;
        }

        public void AddConnexionEvent(ConnexionStatus connexionStatus)
        {
            _connexionHistory.Add(DateTime.Now, connexionStatus);
            UpdateCurrentConnexionStatus();
        }

        private void UpdateCurrentConnexionStatus()
        {
            ConnexionStatus = _connexionHistory.MaxBy(pair => pair.Key).Value;
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
