﻿using System;
using System.Runtime.Serialization;

namespace MySharpChat.Core.Command
{
    [Serializable]
    public class CommandException : ApplicationException
    {
        public CommandException() : base() { }
        public CommandException(string? message) : base(message) { }
        public CommandException(string format, params string?[] args) : base(string.Format(format, args)) { }
        public CommandException(string? message, Exception? innerException) : base(message, innerException) { }
    }
}
