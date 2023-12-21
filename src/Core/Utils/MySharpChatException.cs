using System;

namespace MySharpChat.Core.Utils
{
    [Serializable]
    public class MySharpChatException : ApplicationException
    {
        public MySharpChatException() : base() { }
        public MySharpChatException(string? message) : base(message) { }
        public MySharpChatException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}
