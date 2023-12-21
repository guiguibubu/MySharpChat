using System;
using System.Text.Json.Serialization;

namespace MySharpChat.Core.Event
{
    [Serializable]
    public sealed class UsernameChangeEvent : ChatEvent
    {
        public string OldUsername { get; }
        public string NewUsername { get; }

        public UsernameChangeEvent(string oldUsername, string newUsername)
            : base()
        {
            OldUsername = oldUsername;
            NewUsername = newUsername;
        }

        [JsonConstructor]
        public UsernameChangeEvent(Guid id, DateTime date, string oldUsername, string newUsername)
            : base(id, date)
        {
            OldUsername = oldUsername;
            NewUsername = newUsername;
        }
    }
}
