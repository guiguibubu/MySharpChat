
namespace MySharpChat.Client.Console.UI
{
    public interface IUserInputCursorContext
    {
        int Width { get; }
        int X { get; set; }
        int Y { get; set; }
    }
}
