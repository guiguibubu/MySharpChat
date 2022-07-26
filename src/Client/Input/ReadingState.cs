using System;
using System.IO;

namespace MySharpChat.Client.Input
{
    internal sealed class ReadingState
    {
        public ReadingState(IUserInputTextHandler inputTextHandler, IUserInputCursorHandler cursorHandler, LockTextWriter outputStream)
        {
            ReadingFinished = false;
            InputTextHandler = inputTextHandler;
            CursorHandler = cursorHandler;
            OutputStream = outputStream;
        }

        public bool ReadingFinished { get; set; }
        public int Position => CursorHandler.Position;
        public ConsoleKeyInfo Key { get; set; }
        public LockTextWriter OutputStream { get; }
        public IUserInputCursorHandler CursorHandler { get; }
        public IUserInputTextHandler InputTextHandler { get; }
    }
}
