using MySharpChat.Core.Console;

namespace MySharpChat.Client.Console.UI
{
    internal interface IUserInterfaceModule
    {
        ConsoleInputReader InputModule { get; }
        ConsoleOutputModule OutputModule { get; }
    }
}
