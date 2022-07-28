using System;
using System.Diagnostics;

using MySharpChat.Client.Input;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using MySharpChat.Core.Utils;

namespace MySharpChat.Client.Command
{
    internal static class CommandInput
    {

        private static readonly KeyActionsCollection KeyActions = new KeyActionsCollection();
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
        public static string ReadLine(ReadingState? reading = null)
        {
            ReadingState readingState = reading ?? new ReadingState(new UserInputTextHandler(), new ConsoleCursorHandler(new ConsoleCursorContext()), new ConsoleOutputWriter());

            while (!readingState.ReadingFinished)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                readingState.Key = key;
                if (KeyActions.TryGetValue(key, out KeyActionDelegate? keyAction))
                {
                    keyAction?.Invoke(readingState);
                }
                else
                {
                    DefaultKeyAction(readingState);
                }
            }

            string text = string.Empty;

            if (readingState.InputTextHandler.Length > 0)
            {
                text = readingState.InputTextHandler.ToString();
                if (!string.Equals(text, CommandHistory.LastCommand, StringComparison.InvariantCulture))
                    CommandHistory.Add(text);
                CommandHistory.ResetPosition();
            }

            return text;
        }

        public static Task<string> ReadLineAsync(ReadingState? reading = null, CancellationToken cancelToken = default)
        {
            ReadingState readingState = reading ?? new ReadingState(new UserInputTextHandler(), new ConsoleCursorHandler(new ConsoleCursorContext()), new ConsoleOutputWriter());

            return Task.Factory.StartNew(() => { return ReadLine(readingState); }, cancelToken);
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
            KeyActions.Add(ConsoleKey.Enter,
                (ReadingState readingState) =>
                {
                    readingState.ReadingFinished = true;
                    TextWriter outputStream = readingState.OutputStream;
                    outputStream.WriteLine();
                }
            );
            KeyActions.Add(ConsoleKey.Escape,
                (ReadingState readingState) =>
                {
                    ClearCommand(readingState);
                }
            );
            KeyActions.Add(ConsoleKey.Tab,
                (ReadingState readingState) =>
                {
                    // TODO Add support for auto completion
                    Trace.WriteLine("TABULATION");
                }
            );
            KeyActions.Add(ConsoleKey.RightArrow,
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
            KeyActions.Add(ConsoleKey.RightArrow, ConsoleModifiers.Control,
                (ReadingState readingState) =>
                {
                    IUserInputCursorHandler cursorHandler = readingState.CursorHandler;
                    IUserInputTextHandler inputTextHandler = readingState.InputTextHandler;
                    int textLength = inputTextHandler.Length;
                    if (cursorHandler.Position < textLength)
                    {
                        cursorHandler.MovePositionToTail(textLength);
                    }

                }
            );
            KeyActions.Add(ConsoleKey.LeftArrow,
                (ReadingState readingState) =>
                {
                    IUserInputCursorHandler cursorHandler = readingState.CursorHandler;
                    if (cursorHandler.Position > 0)
                    {
                        cursorHandler.MovePositionNegative(1);
                    }
                }
            );

            KeyActions.Add(ConsoleKey.LeftArrow, ConsoleModifiers.Control,
                (ReadingState readingState) =>
                {
                    IUserInputCursorHandler cursorHandler = readingState.CursorHandler;
                    if (cursorHandler.Position > 0)
                    {
                        cursorHandler.MovePositionToOrigin();
                    }
                }
            );
            KeyActions.Add(ConsoleKey.Backspace,
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
            KeyActions.Add(ConsoleKey.Delete,
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
            KeyActions.Add(ConsoleKey.UpArrow,
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

            KeyActions.Add(ConsoleKey.DownArrow,
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
            cursorHandler.MovePositionPositive(1, CursorUpdateMode.NoGraphic);
        }

        private static void WriteStr(ReadingState readingState, string s)
        {
            foreach (char c in s)
            {
                WriteChar(readingState, c);
            }
        }

        private static void ResetPosition(ReadingState readingState)
        {
            readingState.CursorHandler.MovePositionToOrigin();
        }
    }
}
