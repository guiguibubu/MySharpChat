using System;

namespace MySharpChat.Core.Console
{
    public class ConsoleInputReader
    {
        public ConsoleKeyInfo ReadKey(bool intercept)
        {
            return System.Console.ReadKey(intercept);
        }
    }
}
