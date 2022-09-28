using MySharpChat.Core.Console;
using MySharpChat.Core.Utils;

namespace MySharpChat.Client.Console.UI
{
    internal interface IUserInterfaceModule
    {
        ConsoleInputReader InputModule { get; }
        ConsoleOutputModule OutputModule { get; }
    }
}
