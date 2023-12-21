using System;

namespace MySharpChat.Core
{
    [Serializable]
    public class ConnectionNotInitializedException : ApplicationException
    {
        public ConnectionNotInitializedException() : base() { }
        public ConnectionNotInitializedException(string? message) : base(message) { }
        public ConnectionNotInitializedException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}
