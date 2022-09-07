using MySharpChat.Core.Model;
using System;
using System.Text.Json.Serialization;

namespace MySharpChat.Core.Packet
{
    [Serializable]
    public class UserInfoPacket
    {
        public UserInfoPacket(Guid userId, string username, bool connected) 
            : this(new User(userId, username), connected)
        { }

        [JsonConstructor]
        public UserInfoPacket(User user, bool connected)
        {
            User = user;
            Connected = connected;
        }

        public User User { get; }
        public bool Connected { get; }
    }
}
