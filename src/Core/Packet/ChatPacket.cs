using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySharpChat.Core.Packet
{
    [Serializable]
    public class ChatPacket
    {
        public ChatPacket(string message)
        {
            Message = message;
        }

        public string Message { get; }
    }
}
