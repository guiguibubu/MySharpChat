using MySharpChat.Core.Model;
using System;
using System.Text.Json.Serialization;

namespace MySharpChat.Core.Event
{
    [Serializable]
    public sealed class ChatMessageEvent : ChatEvent
    {
        public ChatMessage ChatMessage { get; }

        public ChatMessageEvent(ChatMessage message) 
            : base()
        {
            ChatMessage = message;
        }

        [JsonConstructor]
        public ChatMessageEvent(Guid id, DateTime date, ChatMessage chatMessage) 
            : base(id, date)
        {
            ChatMessage = chatMessage;
        }
    }
}
