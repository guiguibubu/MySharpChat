using System;
using System.Text.Json.Serialization;
using MySharpChat.Core.Model;

namespace MySharpChat.Core.Packet
{
    [Serializable]
    public class UserInfoPacket
    {
        public UserInfoPacket(Guid userId, string username, ConnexionStatus connexionStatus)
            : this(new User(userId, username), connexionStatus)
        { }

        public UserInfoPacket(User user, ConnexionStatus connexionStatus)
            : this(new UserState(user, connexionStatus))
        { }

        [JsonConstructor]
        public UserInfoPacket(UserState userState)
        {
            UserState = userState;
        }

        public UserState UserState { get; }
    }
}
