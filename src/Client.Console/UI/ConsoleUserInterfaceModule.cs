using MySharpChat.Core.Console;
using MySharpChat.Core.Utils;

namespace MySharpChat.Client.Console.UI
{
    public class ConsoleUserInterfaceModule : IUserInterfaceModule
    {
        public ConsoleUserInterfaceModule(IUserInputCursorHandler cursorHandler, ConsoleInputReader inputReader, LockTextWriter output)
        {
            CursorHandler = cursorHandler;
            InputReader = inputReader;
            OutputWriter = output;
        }

        public ConsoleUserInterfaceModule() : this(new ConsoleCursorHandler(new ConsoleCursorContext()), new ConsoleInputReader(), new ConsoleOutputWriter()) { }

        public IUserInputCursorHandler CursorHandler { get; private set; }

        public ConsoleInputReader InputReader { get; }

        public LockTextWriter OutputWriter { get; private set; }
    }
}
