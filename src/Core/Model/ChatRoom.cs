using System;
using MySharpChat.Core.Utils;
using MySharpChat.Core.Utils.Collection;
using MySharpChat.Core.Utils.Logger;

namespace MySharpChat.Core.Model
{
    public class ChatRoom : IObjectWithId
    {
        private static readonly Logger logger = Logger.Factory.GetLogger<ChatRoom>();

        public UserStateCollection Users { get; private set; } = new();
        public ChatMessageCollection Messages { get; private set; } = new();
        public Guid Id { get; private set; }

        public ChatRoom(Guid id)
        {
            Id = id;
        }
    }
}
