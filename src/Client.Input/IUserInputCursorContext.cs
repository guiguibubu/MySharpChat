namespace MySharpChat.Client.Input
{
    internal interface IUserInputCursorContext
    {
        int Width { get; }
        int X { get; set; }
        int Y { get; set; }
    }
}
