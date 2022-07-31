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
        public ClientInitialisationPacket(Guid sessionId, string username = "")
        {
            SessionId = sessionId;
            Username = username;
        }

        public Guid SessionId { get; set; }
        public string Username { get; set; }
    }
}
