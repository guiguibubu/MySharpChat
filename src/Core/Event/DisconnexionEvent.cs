using MySharpChat.Core.Model;
using System;
using System.Text.Json.Serialization;

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
