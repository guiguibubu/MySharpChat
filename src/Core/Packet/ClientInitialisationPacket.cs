using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySharpChat.Core.Packet
{
    [Serializable]
    public class ClientInitialisationPacket
    {
        public ClientInitialisationPacket(Guid userId, string username = "")
        {
            SessionId = userId;
            Username = username;
        }

        public Guid SessionId { get; set; }
        public string Username { get; set; }
    }
}
