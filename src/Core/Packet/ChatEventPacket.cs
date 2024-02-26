using MySharpChat.Core.Event;
using System;

namespace MySharpChat.Core.Packet
{
    [Serializable]
    public class ChatEventPacketWrapper : PacketWrapper<ChatEvent>
    {
        public ChatEventPacketWrapper(Guid sourceId, ChatEvent chatEvent) 
            : base(sourceId, chatEvent) 
        { }
    }
}
