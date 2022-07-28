using MySharpChat.Core.Console;
using MySharpChat.Core.UI;
using MySharpChat.Core.Utils;

namespace MySharpChat.Client.Console
{
    public class ConsoleUserInterfaceModule : IUserInterfaceModule
    {
        public ConsoleUserInterfaceModule(IUserInputCursorHandler cursorHandler, IInputReader inputReader, LockTextWriter output)
        {
            CursorHandler = cursorHandler;
            InputReader = inputReader;
            OutputWriter = output;
        }

        public ConsoleUserInterfaceModule() : this(new ConsoleCursorHandler(new ConsoleCursorContext()), new ConsoleInputReader(), new ConsoleOutputWriter()) { }

        public IUserInputCursorHandler CursorHandler { get; private set; }

        public IInputReader InputReader { get; }

        public LockTextWriter OutputWriter { get; private set; }
    }
}
