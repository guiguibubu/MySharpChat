using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MySharpChat.Core.Utils
{
    [Serializable]
    public class MySharpChatException : ApplicationException
    {
        public MySharpChatException() : base() { }
        public MySharpChatException(string? message) : base(message) { }
        public MySharpChatException(string? message, Exception? innerException) : base(message, innerException) { }
        protected MySharpChatException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
