using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySharpChat.Core.Packet
{
    [Serializable]
    public class ConnectionInitialisationPacket
    {
        public ConnectionInitialisationPacket(Guid sessionId)
        {
            SessionId = sessionId;
        }

        public Guid SessionId { get; set; }
    }
}
