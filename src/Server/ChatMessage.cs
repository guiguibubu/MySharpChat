using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySharpChat.Server
{
    internal class ChatMessage
    {
        private readonly User _user;
        public User User => _user;

        private readonly string _message;
        public string Message => _message;

        public ChatMessage(User user, string message)
        {
            _user = user;
            _message = message;
        }
    }
}
