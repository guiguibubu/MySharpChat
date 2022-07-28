using MySharpChat.Core.Console;
using MySharpChat.Core.UI;
using MySharpChat.Core.Utils;

namespace MySharpChat.Client.Console
{
    public class ConsoleUserInterfaceModule : IUserInterfaceModule
    {
        public ConsoleUserInterfaceModule(IUserInputCursorHandler cursorHandler, LockTextWriter output)
        {
            CursorHandler = cursorHandler;
            OutputStream = output;
        }

        public ConsoleUserInterfaceModule() : this(new ConsoleCursorHandler(new ConsoleCursorContext()), new ConsoleOutputWriter()) { }

        public IUserInputCursorHandler CursorHandler { get; private set; }

        public LockTextWriter OutputStream { get; private set; }
    }
}
