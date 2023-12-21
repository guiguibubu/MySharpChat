using System;
using System.Collections.Generic;
using MySharpChat.Core.Event;

namespace MySharpChat.Core.Utils.Collection
{
    [Serializable]
    public sealed class ChatEventCollection : ObjectWithIdCollection<ChatEvent>
    {
        public ChatEventCollection()
            : base(ChatEvent.Comparer)
        { }

        public ChatEventCollection(IEnumerable<ChatEvent> collection)
            : base(collection, ChatEvent.Comparer)
        { }

        public ChatEventCollection(int capacity)
            : base(capacity, ChatEvent.Comparer)
        { }
    }
}
