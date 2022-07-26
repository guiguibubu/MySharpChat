namespace MySharpChat.Client.Input
{
    public interface IUserInputCursorContext
    {
        int Width { get; }
        int X { get; set; }
        int Y { get; set; }
    }
}
