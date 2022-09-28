namespace MySharpChat.Client.Console.UI
{
    public class ConsoleCursorContext
    {
        public int Width => System.Console.BufferWidth;
        public int X { get => System.Console.CursorLeft; set => System.Console.CursorLeft = value; }
        public int Y { get => System.Console.CursorTop; set => System.Console.CursorTop = value; }
    }
}
