using System;

namespace MySharpChat.Core.Packet
{
    [Serializable]
    public class UserStatusPacket
    {
        public UserStatusPacket(string username, bool connected)
        {
            Username = username;
            Connected = connected;
        }

        public string Username { get; set; }
        public bool Connected { get; set; }
    }
}
