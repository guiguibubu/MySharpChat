using System;
using MySharpChat.Client.Console.UI;

namespace MySharpChat.Client.Console.Input
{
    internal sealed class ReadingState
    {
        public ReadingState(IUserInputTextHandler inputTextHandler, ConsoleCursorHandler inputCursorHandler, IUserInterfaceModule userInterfaceModule)
        {
            ReadingFinished = false;
            InputTextHandler = inputTextHandler;
            UserInterfaceModule = userInterfaceModule;
            InputCursorHandler = inputCursorHandler;
        }

        public bool ReadingFinished { get; set; }
        public ConsoleKeyInfo Key { get; set; }
        public IUserInterfaceModule UserInterfaceModule { get; }
        public IUserInputTextHandler InputTextHandler { get; }
        public ConsoleCursorHandler InputCursorHandler { get; }
    }
}
