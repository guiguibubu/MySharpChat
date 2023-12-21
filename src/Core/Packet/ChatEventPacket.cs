using System;
using MySharpChat.Core.Event;

namespace MySharpChat.Core.Packet
{
    [Serializable]
    public class ChatEventPacketWrapper : PacketWrapper
    {
        public ChatEventPacketWrapper(Guid sourceId, ChatEvent chatEvent)
            : base(sourceId, chatEvent)
        { }
    }
}
