using MySharpChat.Core.Utils;
using MySharpChat.Core.Utils.Logger;
using System;
using System.Collections.Generic;

namespace MySharpChat.Core.Model
{
    public class ChatRoom : IObjectWithId
    {
        private static readonly Logger logger = Logger.Factory.GetLogger<ChatRoom>();

        public ObjectWithIdCollection<UserState> Users { get; private set; } = new(UserState.Comparer);
        public ObjectWithIdCollection<ChatMessage> Messages { get; private set; } = new(ChatMessage.Comparer);
        public Guid Id { get; private set; }

        public ChatRoom(Guid id)
        {
            Id = id;
        }
    }
}
