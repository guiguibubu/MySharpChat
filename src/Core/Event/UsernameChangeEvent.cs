using System;
using System.Text.Json.Serialization;

namespace MySharpChat.Core.Event
{
    [Serializable]
    public sealed class UsernameChangeEvent : ChatEvent
    {
        public Guid UidUser { get; }
        public string OldUsername { get; }
        public string NewUsername { get; }

        public UsernameChangeEvent(Guid uidUser, string oldUsername, string newUsername) 
            : base()
        {
            UidUser = uidUser;
            OldUsername = oldUsername;
            NewUsername = newUsername;
        }

        [JsonConstructor]
        public UsernameChangeEvent(Guid id, DateTime date, Guid uidUser, string oldUsername, string newUsername) 
            : base(id, date)
        {
            UidUser = uidUser;
            OldUsername = oldUsername;
            NewUsername = newUsername;
        }
    }
}
