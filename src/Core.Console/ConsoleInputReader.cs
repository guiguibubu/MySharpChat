using System;
using MySharpChat.Core.UI;

namespace MySharpChat.Core.Console
{
    public class ConsoleInputReader : IInputReader
    {
        public ConsoleKeyInfo ReadKey(bool intercept)
        {
            return System.Console.ReadKey(intercept);
        }
    }
}
