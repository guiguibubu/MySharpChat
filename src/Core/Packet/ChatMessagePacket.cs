using MySharpChat.Core.Model;
using System;
using System.Text.Json.Serialization;

namespace MySharpChat.Core.Packet
{
    [Serializable]
    public class ChatMessagePacket
    {
        public ChatMessagePacket(Guid id, User user, DateTime date, string message) 
            : this(new ChatMessage(id, user, date, message))
        { }

        [JsonConstructor]
        public ChatMessagePacket(ChatMessage chatMessage)
        {
            ChatMessage = chatMessage;
        }

        public ChatMessage ChatMessage { get; }
    }
}
