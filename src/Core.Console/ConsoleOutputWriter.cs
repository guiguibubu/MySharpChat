using MySharpChat.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace MySharpChat.Core.Console
{
    public class ConsoleOutputWriter : LockTextWriter
    {
        public ConsoleOutputWriter(TextWriter output) : base(output)
        {
        }

        public ConsoleOutputWriter() : this(System.Console.Out) { }
    }
}
