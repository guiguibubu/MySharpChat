using System;
using System.Collections.Generic;
using MySharpChat.Core.Model;

namespace MySharpChat.Core.Utils.Collection
{
    [Serializable]
    public sealed class ChatMessageCollection : ObjectWithIdCollection<ChatMessage>
    {
        public ChatMessageCollection()
            : base(ChatMessage.Comparer)
        { }

        public ChatMessageCollection(IEnumerable<ChatMessage> collection)
            : base(collection, ChatMessage.Comparer)
        { }

        public ChatMessageCollection(int capacity)
            : base(capacity, ChatMessage.Comparer)
        { }
    }
}
