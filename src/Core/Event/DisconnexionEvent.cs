using System;
using System.Text.Json.Serialization;
using MySharpChat.Core.Model;

namespace MySharpChat.Core.Event
{
    [Serializable]
    public sealed class DisconnexionEvent : ChatEvent
    {
        public User User { get; }

        public DisconnexionEvent(User user)
            : base()
        {
            User = user;
        }

        [JsonConstructor]
        public DisconnexionEvent(Guid id, DateTime date, User user)
            : base(id, date)
        {
            User = user;
        }
    }
}
