using MySharpChat.Core.UI;
using MySharpChat.Core.Utils;

namespace MySharpChat.Client.GUI
{
    internal class GuiUserInterfaceModule : IUserInterfaceModule
    {
        public IUserInputCursorHandler CursorHandler { get; private set; }

        public IInputReader InputReader { get; }

        public LockTextWriter OutputWriter { get; private set; }

        public GuiUserInterfaceModule(IUserInputCursorHandler cursorHandler, IInputReader inputReader, LockTextWriter output)
        {
            CursorHandler = cursorHandler;
            InputReader = inputReader;
            OutputWriter = output;
        }
    }
}
