using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MySharpChat.Client.Console.Input;
using MySharpChat.Client.Console.UI;
using MySharpChat.Core.Console;

namespace MySharpChat.Client.Console.Command
{
    internal class ConsoleCommandInput
    {

        private static readonly KeyActionsCollection KeyActions = new KeyActionsCollection();
        private static readonly CommandHistoryCollection CommandHistory = new CommandHistoryCollection();

        public event Action? OnInputChanged;

        private readonly IUserInterfaceModule _userInterfaceModule;

        private ConsoleCursorHandler InputCursorHandler { get; }

        static ConsoleCommandInput()
        {
            InitializeKeyActions();
        }

        public ConsoleCommandInput(IUserInterfaceModule userInterfaceModule)
            : this(userInterfaceModule, new ConsoleCursorHandler())
        { }

        public ConsoleCommandInput(IUserInterfaceModule userInterfaceModule, ConsoleCursorHandler inputCursorHandler)
        {
            _userInterfaceModule = userInterfaceModule;
            InputCursorHandler = inputCursorHandler;
        }

        //TODO: Better handle of user input
        /// <summary>
        /// Reads a line. A line is defined as a sequence of characters followed by a carriage return ('\r'), a line feed ('\n'), or a carriage return
        /// immediately followed by a line feed. The resulting string does not
        /// contain the terminating carriage return and/or line feed. The returned
        /// value is null if the end of the input stream has been reached.
        /// </summary>
        /// <returns></returns>
        public string ReadLine(ReadingState? reading = null)
        {
            ReadingState readingState = reading ?? new ReadingState(new UserInputTextHandler(), InputCursorHandler, _userInterfaceModule);
            ConsoleInputReader inputReader = _userInterfaceModule.InputModule;

            while (!readingState.ReadingFinished)
            {
                ConsoleKeyInfo key = inputReader.ReadKey(true);
                readingState.Key = key;
                if (KeyActions.TryGetValue(key, out KeyActionDelegate? keyAction))
                {
                    keyAction?.Invoke(readingState);
                }
                else
                {
                    DefaultKeyAction(readingState);
                }

                if (readingState.InputTextHandler.Length >= 0)
                {
                    OnInputChanged?.Invoke();
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

            readingState.ReadingFinished = false;

            ClearCommand(readingState);

            return text;
        }

        public Task<string> ReadLineAsync(ReadingState? reading = null, CancellationToken cancelToken = default)
        {
            ReadingState readingState = reading ?? new ReadingState(new UserInputTextHandler(), InputCursorHandler, _userInterfaceModule);

            return Task.Factory.StartNew(() => { return ReadLine(readingState); }, cancelToken);
        }

        private static void DefaultKeyAction(ReadingState readingState)
        {
            IUserInputTextHandler inputTextHandler = readingState.InputTextHandler;
            ConsoleCursorHandler inputCursorHandler = readingState.InputCursorHandler;
            ConsoleKeyInfo key = readingState.Key;

            int oldPosition = inputCursorHandler.Position;
            int oldLength = inputTextHandler.Length;
            char newChar = key.KeyChar;

            if (oldPosition == oldLength)
            {
                inputTextHandler.Append(newChar);
            }
            else
            {
                inputTextHandler.InsertAt(oldPosition, newChar);
            }

            inputCursorHandler.MovePositionPositive(1);
        }

        private static void InitializeKeyActions()
        {
            KeyActions.Add(ConsoleKey.Enter,
                (ReadingState readingState) =>
                {
                    readingState.ReadingFinished = true;
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
                    IUserInputTextHandler inputTextHandler = readingState.InputTextHandler;
                    ConsoleCursorHandler inputCursorHandler = readingState.InputCursorHandler;
                    int textLength = inputTextHandler.Length;
                    if (inputCursorHandler.Position < textLength)
                    {
                        inputCursorHandler.MovePositionPositive(1);
                    }

                }
            );
            KeyActions.Add(ConsoleKey.RightArrow, ConsoleModifiers.Control,
                (ReadingState readingState) =>
                {
                    IUserInputTextHandler inputTextHandler = readingState.InputTextHandler;
                    ConsoleCursorHandler inputCursorHandler = readingState.InputCursorHandler;
                    int textLength = inputTextHandler.Length;
                    if (inputCursorHandler.Position < textLength)
                    {
                        inputCursorHandler.MovePositionToTail(textLength);
                    }

                }
            );
            KeyActions.Add(ConsoleKey.LeftArrow,
                (ReadingState readingState) =>
                {
                    ConsoleCursorHandler inputCursorHandler = readingState.InputCursorHandler;
                    if (inputCursorHandler.Position > 0)
                    {
                        inputCursorHandler.MovePositionNegative(1);
                    }
                }
            );

            KeyActions.Add(ConsoleKey.LeftArrow, ConsoleModifiers.Control,
                (ReadingState readingState) =>
                {
                    ResetPosition(readingState);
                }
            );
            KeyActions.Add(ConsoleKey.Backspace,
                (ReadingState readingState) =>
                {
                    IUserInputTextHandler inputTextHandler = readingState.InputTextHandler;
                    ConsoleCursorHandler inputCursorHandler = readingState.InputCursorHandler;
                    int oldPosition = inputCursorHandler.Position;
                    int newPosition = oldPosition - 1;

                    if (oldPosition > 0)
                    {
                        inputTextHandler.RemoveAt(newPosition);
                        inputCursorHandler.MovePositionNegative(1);
                    }
                }
            );
            KeyActions.Add(ConsoleKey.Delete,
                (ReadingState readingState) =>
                {
                    IUserInputTextHandler inputTextHandler = readingState.InputTextHandler;
                    ConsoleCursorHandler inputCursorHandler = readingState.InputCursorHandler;
                    int textLength = inputTextHandler.Length;
                    int oldPosition = inputCursorHandler.Position;

                    if (oldPosition < textLength)
                    {
                        inputTextHandler.RemoveAt(oldPosition);
                        inputCursorHandler.MovePositionNegative(1);
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
                        readingState.InputCursorHandler.MovePositionPositive(oldCommand!.Length);
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
                        readingState.InputCursorHandler.MovePositionPositive(oldCommand!.Length);
                    }
                }
            );
        }

        public static void ClearCommand(ReadingState readingState)
        {
            IUserInputTextHandler inputTextHandler = readingState.InputTextHandler;

            inputTextHandler.Clear();
            ResetPosition(readingState);
        }

        private static void ResetPosition(ReadingState readingState)
        {
            ConsoleCursorHandler inputCursorHandler = readingState.InputCursorHandler;
            inputCursorHandler.MovePositionToOrigin();
        }
    }
}
