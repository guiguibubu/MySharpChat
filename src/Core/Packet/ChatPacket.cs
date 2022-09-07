using MySharpChat.Core.Model;
using MySharpChat.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MySharpChat.Core.Packet
{
    [Serializable]
    public class ChatPacket
    {
        public ChatPacket(Guid id, User user, string message) 
            : this(new ChatMessage(id, user, message))
        { }

        [JsonConstructor]
        public ChatPacket(ChatMessage chatMessage)
        {
            ChatMessage = chatMessage;
        }

        public ChatMessage ChatMessage { get; }
    }
}
