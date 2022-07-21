using System;

namespace MySharpChat.Client.Input
{
    internal class ConsoleCursorContext : IUserInputCursorContext
    {
        public int Width => Console.BufferWidth;
        public int X { get => Console.CursorLeft; set => Console.CursorLeft = value; }
        public int Y { get => Console.CursorTop; set => Console.CursorTop =value; }
    }
}
