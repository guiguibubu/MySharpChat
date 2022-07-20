using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Runtime.InteropServices;

namespace MySharpChat.Client.Command
{
    public static class CommandInput
    {

        private static int CurrentWidth { get { return Console.BufferWidth; } }
        private static int CurrentHeight { get { return Console.BufferHeight; } }
        private static int CurrentPositionX { get { return Console.CursorLeft; } }
        private static int CurrentPositionY { get { return Console.CursorTop; } }
        private static readonly Dictionary<ConsoleKey, KeyAction> KeyActions = InitializeKeyActions();
        private static readonly CommandHistoryCollection CommandHistory = new CommandHistoryCollection();

        static CommandInput()
        {
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
            ReadingState readingState = new ReadingState(CurrentPositionX, CurrentPositionY, CurrentWidth, CurrentHeight, Console.WindowWidth, Console.WindowHeight);

            while (!readingState.ReadingFinished)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                if (CurrentHeight != readingState.InitialHeigth || CurrentWidth != readingState.InitialWidth)
                {
                    HandleConsoleResize(readingState);
                }
                if (KeyActions.TryGetValue(key.Key, out KeyAction? keyAction))
                {
                    keyAction?.Action?.Invoke(ref readingState);
                }
                else
                {
                    int currentPositionInText = GetCurrentPositionInText(CurrentPositionX, CurrentPositionY, readingState.InitialPositionX, readingState.InitialPositionY);
                    readingState.sb.Insert(currentPositionInText, key.KeyChar);
                    UpdateRendering(readingState);
                    MovePositionInText(1);
                }
            }

            string? text = null;

            if (readingState.sb.Length > 0)
            {
                text = readingState.sb.ToString();
                if (!string.Equals(text, CommandHistory.CurrentCommand, StringComparison.InvariantCulture))
                    CommandHistory.Add(text);
                CommandHistory.ResetPosition();
            }

            return text;
        }

        private static void HandleConsoleResize(ReadingState readingState)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                HandleConsoleResizeWindows(readingState);
            }
        }

        [SupportedOSPlatform("windows")]
        private static void HandleConsoleResizeWindows(ReadingState readingState)
        {
            Console.SetBufferSize(readingState.InitialWidth, readingState.InitialHeigth);
            Console.SetWindowSize(readingState.InitialWindowWidth, readingState.InitialWindowHeigth);
        }

        private static Dictionary<ConsoleKey, KeyAction> InitializeKeyActions()
        {
            Dictionary<ConsoleKey, KeyAction> keyActions = new Dictionary<ConsoleKey, KeyAction>();
            RegisterKeyAction(keyActions, ConsoleKey.Enter,
                (ref ReadingState readingState) =>
                {
                    readingState.ReadingFinished = true;
                    Console.WriteLine();
                }
            );
            RegisterKeyAction(keyActions, ConsoleKey.Escape,
                (ref ReadingState readingState) =>
                {
                    ClearCommand(ref readingState);
                }
            );
            RegisterKeyAction(keyActions, ConsoleKey.Tab,
                (ref ReadingState readingState) =>
                {
                    Trace.WriteLine("TABULATION");
                }
            );
            RegisterKeyAction(keyActions, ConsoleKey.RightArrow,
                (ref ReadingState readingState) =>
                {
                    int currentPositionInText = GetCurrentPositionInText(CurrentPositionX, CurrentPositionY, readingState.InitialPositionX, readingState.InitialPositionY);
                    if (currentPositionInText < readingState.sb.Length)
                    {
                        MovePositionInText(1);
                    }

                }
            );
            RegisterKeyAction(keyActions, ConsoleKey.LeftArrow,
                (ref ReadingState readingState) =>
                {
                    int currentPositionInText = GetCurrentPositionInText(CurrentPositionX, CurrentPositionY, readingState.InitialPositionX, readingState.InitialPositionY);
                    if (currentPositionInText > 0)
                    {
                        MovePositionInText(-1);
                    }
                }
            );
            RegisterKeyAction(keyActions, ConsoleKey.Backspace,
                (ref ReadingState readingState) =>
                {
                    int currentPositionInText = GetCurrentPositionInText(CurrentPositionX, CurrentPositionY, readingState.InitialPositionX, readingState.InitialPositionY);
                    if (currentPositionInText > 0)
                    {
                        MovePositionInText(-1);
                        currentPositionInText = GetCurrentPositionInText(CurrentPositionX, CurrentPositionY, readingState.InitialPositionX, readingState.InitialPositionY);
                        readingState.sb.Remove(currentPositionInText, 1);

                        UpdateRendering(readingState);
                    }
                }
            );
            RegisterKeyAction(keyActions, ConsoleKey.Delete,
                (ref ReadingState readingState) =>
                {
                    int currentPositionInText = GetCurrentPositionInText(CurrentPositionX, CurrentPositionY, readingState.InitialPositionX, readingState.InitialPositionY);
                    if (currentPositionInText < readingState.sb.Length)
                    {
                        ClearCommand(ref readingState, true);

                        readingState.sb.Remove(currentPositionInText, 1);

                        UpdateRendering(readingState);

                        MovePositionInText(currentPositionInText);
                    }
                }
            );
            RegisterKeyAction(keyActions, ConsoleKey.UpArrow,
                (ref ReadingState readingState) =>
                {
                    if (CommandHistory.TryGetPreviousCommand(out string? oldCommand))
                    {
                        ClearCommand(ref readingState);
                        readingState.sb.Append(oldCommand);
                        Console.Write(oldCommand);
                    }
                }
            );

            RegisterKeyAction(keyActions, ConsoleKey.DownArrow,
                (ref ReadingState readingState) =>
                {
                    if (CommandHistory.TryGetNextCommand(out string? oldCommand))
                    {
                        ClearCommand(ref readingState);
                        readingState.sb.Append(oldCommand);
                        Console.Write(oldCommand);
                    }
                }
            );

            return keyActions;
        }

        private static int GetCurrentPositionInText(int x, int y, int initX, int initY)
        {
            int currentPositionInText = 0;
            int dy = y - initY;
            if (dy == 0)
                currentPositionInText = x - initX;
            else if (dy == 1)
                currentPositionInText = x + (Console.BufferWidth - initX);
            else
                currentPositionInText = x + (Console.BufferWidth - initX) + (dy - 1) * Console.BufferWidth;
            return currentPositionInText;
        }

        // TODO !!!
        private static void MovePositionInText(int move)
        {
            int newX = CurrentPositionX + move;
            int bufferWidth = Console.BufferWidth;

            int moveX;
            int moveY;

            if (newX >= bufferWidth)
            {
                moveY = newX / bufferWidth;
                moveX = newX % bufferWidth - CurrentPositionX;
            }
            else if (newX < 0)
            {
                moveY = (newX / bufferWidth) - 1;
                moveX = (bufferWidth - CurrentPositionX) - (-newX % bufferWidth);
            }
            else
            {
                moveX = move;
                moveY = 0;
            }

            MoveCursorX(moveX);
            MoveCursorY(moveY);

        }

        private static void MoveCursorX(int move)
        {
            int newPosition = CurrentPositionX + move;
            if (newPosition >= 0)
                Console.CursorLeft = newPosition;
        }

        private static void MoveCursorY(int move)
        {
            int newPosition = CurrentPositionY + move;
            if (newPosition >= 0)
                Console.CursorTop = newPosition;
        }

        private static void ClearCommand(ref ReadingState readingState, bool cursorOnly = false)
        {
            int oldStringSize = readingState.sb.Length;
            if (!cursorOnly)
                readingState.sb.Clear();

            // Rewrite the text in console to shift the tail of the word if deletion in middle of a word
            ResetPosition(readingState);
            for (int i = 0; i < oldStringSize; i++)
                Console.Write(" ");
            ResetPosition(readingState);
        }

        private static void UpdateRendering(ReadingState readingState)
        {
            int currentPositionInText = GetCurrentPositionInText(CurrentPositionX, CurrentPositionY, readingState.InitialPositionX, readingState.InitialPositionY);
            ClearCommand(ref readingState, true);
            Console.Write(readingState.sb.ToString());
            // Move cursor to previous position
            MovePositionInText(currentPositionInText - readingState.sb.Length);
        }

        private static void ResetPosition(ReadingState readingState)
        {
            MoveCursorX(readingState.InitialPositionX - CurrentPositionX);
            MoveCursorY(readingState.InitialPositionY - CurrentPositionY);
        }

        private static void RegisterKeyAction(Dictionary<ConsoleKey, KeyAction> keyActions, ConsoleKey key, KeyActionDelegate action)
        {
            keyActions.Add(key, new KeyAction(key, action));
        }

        private struct ReadingState
        {
            public ReadingState(int initialPositionX, int initialPositionY, int initialWidth, int initialHeigth, int initialWindowWidth, int initialWindowHeigth)
            {
                sb = new StringBuilder();
                ReadingFinished = false;
                InitialPositionX = initialPositionX;
                InitialPositionY = initialPositionY;
                InitialWidth = initialWidth;
                InitialHeigth = initialHeigth;
                InitialWindowWidth = initialWindowWidth;
                InitialWindowHeigth = initialWindowHeigth;
            }

            public readonly StringBuilder sb;
            public bool ReadingFinished { get; set; }
            public int InitialPositionX { get; }
            public int InitialPositionY { get; }
            public int InitialWidth { get; }
            public int InitialHeigth { get; }
            public int InitialWindowWidth { get; }
            public int InitialWindowHeigth { get; }
        }

        private sealed class KeyAction
        {
            public KeyAction(ConsoleKey key, KeyActionDelegate action)
            {
                Key = key;
                Action = action;
            }
            public ConsoleKey Key { get; }
            public KeyActionDelegate Action { get; }
        }

        private delegate void KeyActionDelegate(ref ReadingState readingState);
    }
}
