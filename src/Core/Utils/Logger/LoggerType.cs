using System;

namespace MySharpChat.Core.Utils.Logger
{
    [Flags]
    public enum LoggerType
    {
        None = 0,
        Console = 2 << 0,
        File = 2 << 1,
        Both = Console | File,
    }
}
