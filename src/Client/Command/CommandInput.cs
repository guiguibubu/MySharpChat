using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Runtime.InteropServices;

using MySharpChat.Client.Input;
using System.IO;

namespace MySharpChat.Client.Command
{
    public static class CommandInput
    {

        private static readonly Dictionary<ConsoleKey, KeyActionDelegate> KeyActions = new Dictionary<ConsoleKey, KeyActionDelegate>();
        private static readonly KeyActionDelegate DefaultKeyAction = DefaultKeyActionImpl;
        private static readonly CommandHistoryCollection CommandHistory = new CommandHistoryCollection();

        static CommandInput()
        {
            InitializeKeyActions();
        }

        //TODO: Better handle of user input
        /// <summary>
        /// Reads a line. A line is defined as a sequence of characters followed by a carriage return ('\r'), a line feed ('\n'), or a carriage return
        /// immediately followed by a line feed. The resulting string does not
        /// contain the terminating carriage return and/or line feed. The returned
        /// value is null if the end of the input stream has been reached.
        /// </summary>
        /// <returns></returns>
        public static string? ReadLine()
        {
            ReadingState readingState = new ReadingState(new UserInputTextHandler(), new ConsoleCursorHandler(new ConsoleCursorContext()), Console.Out);

            while (!readingState.ReadingFinished)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                readingState.Key = key;
                if (KeyActions.TryGetValue(key.Key, out KeyActionDelegate? keyAction))
                {
                    keyAction?.Invoke(readingState);
                }
                else
                {
                    DefaultKeyAction(readingState);
                }
            }

            string? text = null;

            if (readingState.InputTextHandler.Length > 0)
            {
                text = readingState.InputTextHandler.ToString();
                if (!string.Equals(text, CommandHistory.LastCommand, StringComparison.InvariantCulture))
                    CommandHistory.Add(text);
                CommandHistory.ResetPosition();
            }

            return text;
        }

        private static void DefaultKeyActionImpl(ReadingState readingState)
        {
            IUserInputCursorHandler cursorHandler = readingState.CursorHandler;
            IUserInputTextHandler inputTextHandler = readingState.InputTextHandler;
            ConsoleKeyInfo key = readingState.Key;

            int oldPosition = cursorHandler.Position;
            int oldLength = inputTextHandler.Length;
            char newChar = key.KeyChar;

            if (oldPosition == oldLength)
            {
                WriteChar(readingState, newChar);
                inputTextHandler.Append(newChar);
            }
            else
            {
                inputTextHandler.InsertAt(oldPosition, newChar);

                string wordTail = inputTextHandler.ToString().Substring(oldPosition);
                WriteStr(readingState, wordTail);

                cursorHandler.MovePositionNegative(cursorHandler.Position - (oldPosition + 1));
            }
        }

        private static void InitializeKeyActions()
        {
            RegisterKeyAction(KeyActions, ConsoleKey.Enter,
                (ReadingState readingState) =>
                {
                    readingState.ReadingFinished = true;
                    Console.WriteLine();
                }
            );
            RegisterKeyAction(KeyActions, ConsoleKey.Escape,
                (ReadingState readingState) =>
                {
                    ClearCommand(readingState);
                }
            );
            RegisterKeyAction(KeyActions, ConsoleKey.Tab,
                (ReadingState readingState) =>
                {
                    Trace.WriteLine("TABULATION");
                }
            );
            RegisterKeyAction(KeyActions, ConsoleKey.RightArrow,
                (ReadingState readingState) =>
                {
                    IUserInputCursorHandler cursorHandler = readingState.CursorHandler;
                    IUserInputTextHandler inputTextHandler = readingState.InputTextHandler;
                    if (cursorHandler.Position < inputTextHandler.Length)
                    {
                        cursorHandler.MovePositionPositive(1);
                    }

                }
            );
            RegisterKeyAction(KeyActions, ConsoleKey.LeftArrow,
                (ReadingState readingState) =>
                {
                    IUserInputCursorHandler cursorHandler = readingState.CursorHandler;
                    if (cursorHandler.Position > 0)
                    {
                        cursorHandler.MovePositionNegative(1);
                    }
                }
            );
            RegisterKeyAction(KeyActions, ConsoleKey.Backspace,
                (ReadingState readingState) =>
                {
                    IUserInputCursorHandler cursorHandler = readingState.CursorHandler;
                    IUserInputTextHandler inputTextHandler = readingState.InputTextHandler;
                    int textLength = inputTextHandler.Length;
                    int oldPosition = cursorHandler.Position;
                    int newPosition = oldPosition - 1;

                    if (oldPosition > 0)
                    {
                        if (oldPosition == textLength)
                        {
                            cursorHandler.MovePositionNegative(1);
                            WriteChar(readingState, ' ');
                            cursorHandler.MovePositionNegative(1);
                        }
                        else
                        {
                            string wordTail = inputTextHandler.ToString().Substring(oldPosition);

                            cursorHandler.MovePositionNegative(1);
                            WriteStr(readingState, wordTail + " ");
                            cursorHandler.MovePositionNegative(cursorHandler.Position - newPosition);
                        }

                        inputTextHandler.RemoveAt(newPosition);

                    }
                }
            );
            RegisterKeyAction(KeyActions, ConsoleKey.Delete,
                (ReadingState readingState) =>
                {
                    IUserInputCursorHandler cursorHandler = readingState.CursorHandler;
                    IUserInputTextHandler inputTextHandler = readingState.InputTextHandler;
                    int textLength = inputTextHandler.Length;
                    int oldPosition = cursorHandler.Position;

                    if (oldPosition < textLength)
                    {
                        inputTextHandler.RemoveAt(oldPosition);
                        string wordTail = inputTextHandler.ToString().Substring(oldPosition);

                        WriteStr(readingState, wordTail + " ");
                        cursorHandler.MovePositionNegative(cursorHandler.Position - oldPosition);
                        
                    }
                }
            );
            RegisterKeyAction(KeyActions, ConsoleKey.UpArrow,
                (ReadingState readingState) =>
                {
                    if (CommandHistory.TryGetPreviousCommand(out string? oldCommand))
                    {
                        ClearCommand(readingState);
                        readingState.InputTextHandler.Append(oldCommand!);
                        WriteStr(readingState, oldCommand!);
                    }
                }
            );

            RegisterKeyAction(KeyActions, ConsoleKey.DownArrow,
                (ReadingState readingState) =>
                {
                    if (CommandHistory.TryGetNextCommand(out string? oldCommand))
                    {
                        ClearCommand(readingState);
                        readingState.InputTextHandler.Append(oldCommand!);
                        WriteStr(readingState, oldCommand!);
                    }
                }
            );
        }

        private static void ClearCommand(ReadingState readingState)
        {
            IUserInputTextHandler inputTextHandler = readingState.InputTextHandler;
            int oldStringSize = inputTextHandler.Length;

            inputTextHandler.Clear();

            ResetPosition(readingState);
            for (int i = 0; i < oldStringSize; i++)
                WriteChar(readingState, ' ');
            ResetPosition(readingState);
        }

        private static void WriteChar(ReadingState readingState, char c)
        {
            IUserInputCursorHandler cursorHandler = readingState.CursorHandler;
            TextWriter outputStream = readingState.OutputStream;
            outputStream.Write(c);
            cursorHandler.MovePositionPositive(1, CursorUpdateModeEnum.NoGraphic);
        }

        private static void WriteStr(ReadingState readingState, string s)
        {
            foreach(char c in s)
            {
                WriteChar(readingState, c);
            }
        }

        private static void ResetPosition(ReadingState readingState)
        {
            readingState.CursorHandler.MovePositionToOrigin();
        }

        private static void RegisterKeyAction(Dictionary<ConsoleKey, KeyActionDelegate> keyActions, ConsoleKey key, KeyActionDelegate action)
        {
            if (keyActions.ContainsKey(key))
                keyActions[key] = action;
            else
                keyActions.Add(key, action);
        }

        private sealed class ReadingState
        {
            public ReadingState(IUserInputTextHandler inputTextHandler, IUserInputCursorHandler cursorHandler, TextWriter outputStream)
            {
                ReadingFinished = false;
                InputTextHandler = inputTextHandler;
                CursorHandler = cursorHandler;
                OutputStream = outputStream;
            }

            public bool ReadingFinished { get; set; }
            public int Position => CursorHandler.Position;
            public ConsoleKeyInfo Key { get; set; }
            public TextWriter OutputStream { get; }
            public IUserInputCursorHandler CursorHandler { get; }
            public IUserInputTextHandler InputTextHandler { get; }
        }

        private delegate void KeyActionDelegate(ReadingState readingState);
    }
}
