using MySharpChat.Client.Console.UI;
using System;

namespace MySharpChat.Client.Console.Input
{
    public sealed class ReadingState
    {
        public ReadingState(IUserInputTextHandler inputTextHandler, IUserInterfaceModule userInterfaceModule)
        {
            ReadingFinished = false;
            InputTextHandler = inputTextHandler;
            UserInterfaceModule = userInterfaceModule;
        }

        public bool ReadingFinished { get; set; }
        public ConsoleKeyInfo Key { get; set; }
        public IUserInterfaceModule UserInterfaceModule { get; }
        public IUserInputTextHandler InputTextHandler { get; }
    }
}
