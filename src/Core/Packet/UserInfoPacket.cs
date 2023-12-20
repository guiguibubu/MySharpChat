﻿using MySharpChat.Core.Model;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MySharpChat.Core.Packet
{
    [Serializable]
    public class UserInfoPacket
    {
        public UserInfoPacket(Guid userId, string username, Dictionary<DateTime, ConnexionStatus> connexionHistory)
            : this(new User(userId, username), connexionHistory)
        { }

        public UserInfoPacket(Guid userId, string username, ConnexionStatus connexionStatus)
            : this(new User(userId, username), connexionStatus)
        { }

        public UserInfoPacket(User user, Dictionary<DateTime, ConnexionStatus> connexionHistory)
            : this(new UserState(user, connexionHistory))
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
