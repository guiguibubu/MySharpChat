using MySharpChat.Core.Event;
using System;
using System.Collections.Generic;
using System.Linq;

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


        public IOrderedEnumerable<ChatEvent> OrderedList => this.OrderBy(e => e.Date);
    }
}
