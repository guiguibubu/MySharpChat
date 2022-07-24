using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
