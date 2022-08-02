using MySharpChat.Core.Console;
using MySharpChat.Core.Utils;

namespace MySharpChat.Client.Console.UI
{
    public interface IUserInterfaceModule
    {
        IUserInputCursorHandler CursorHandler { get; }
        ConsoleInputReader InputReader { get; }
        LockTextWriter OutputWriter { get; }
    }
}
