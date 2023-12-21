using System;
using System.Text.Json.Serialization;
using MySharpChat.Core.Model;

namespace MySharpChat.Core.Event
{
    [Serializable]
    public sealed class ConnexionEvent : ChatEvent
    {
        public User User { get; }

        public ConnexionEvent(User user)
            : base()
        {
            User = user;
        }

        [JsonConstructor]
        public ConnexionEvent(Guid id, DateTime date, User user)
            : base(id, date)
        {
            User = user;
        }
    }
}
