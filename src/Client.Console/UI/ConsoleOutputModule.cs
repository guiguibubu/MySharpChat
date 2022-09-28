using MySharpChat.Core.Console;
using MySharpChat.Core.Utils;
using System;
using System.IO;
using System.Text;

namespace MySharpChat.Client.Console.UI
{
    internal class ConsoleOutputModule
    {
        private ConsoleCursorContext CursorContext { get; }

        private ConsoleCursorHandler OutputCursorHandler { get; }

        public ConsoleOutputWriter OutputWriter { get; }
        public ConsoleOutputWriter ErrorWriter { get; }
        public ConsoleOutputWriter InputWriter { get; }

        private readonly StringBuilder _outputStringBuilder;
        private readonly StringBuilder _errorStringBuilder;
        private readonly StringBuilder _inputStringBuilder;

        private string _inputPrefix = string.Empty;
        private int _inputPosition = 0;

        private bool needToRefresh = false;

        private ConsoleOutputModule(StringBuilder outputStringBuilder, StringBuilder errorStringBuilder, StringBuilder inputStringBuilder)
        {
            _outputStringBuilder = outputStringBuilder;
            _errorStringBuilder = errorStringBuilder;
            _inputStringBuilder = inputStringBuilder;
            OutputWriter = new ConsoleOutputWriter(new StringWriter(_outputStringBuilder));
            ErrorWriter = new ConsoleOutputWriter(new StringWriter(_errorStringBuilder));
            InputWriter = new ConsoleOutputWriter(new StringWriter(_inputStringBuilder));
            CursorContext = new ConsoleCursorContext();
            OutputCursorHandler = new ConsoleCursorHandler();
        }

        public ConsoleOutputModule() : this(new StringBuilder(), new StringBuilder(), new StringBuilder())
        { }

        public void Initialize()
        {
            System.Console.BufferHeight = System.Console.WindowHeight;
            System.Console.BufferWidth = System.Console.WindowWidth;

            System.Console.Clear();

            System.Console.SetCursorPosition(0, 0);
        }

        private static void Clear(StringBuilder stringBuilder)
        {
            stringBuilder.Clear();
        }

        public void ClearOutput()
        {
            Clear(_outputStringBuilder);
        }

        public void ClearError()
        {
            Clear(_errorStringBuilder);
        }

        public void ClearInput()
        {
            Clear(_inputStringBuilder);
        }

        public void Clear()
        {
            ClearOutput();
            ClearInput();
            ClearError();
        }

        public void Refresh(bool force = false)
        {
            if (needToRefresh || force)
            {
                System.Console.Clear();
                System.Console.SetCursorPosition(0, 0);

                OutputWriter.Flush();
                ErrorWriter.Flush();

                int inputY = System.Console.BufferHeight - 2;
                System.Console.SetCursorPosition(0, inputY);

                System.Console.Write(_inputPrefix);
                int inputTextLength = InputWriter.ToString().Length;
                InputWriter.Flush();

                int oldPositionX = CursorContext.X;
                int oldPositionY = CursorContext.Y;

                int width = CursorContext.Width;
                int currentPosition = oldPositionY * width + oldPositionX;
                int newPosition = currentPosition - inputTextLength + _inputPosition;

                int newPositionX = newPosition % width;
                int newPositionY = newPosition / width;

                System.Console.SetCursorPosition(newPositionX, newPositionY);

                needToRefresh = false;
            }
        }

        #region Writer
        public IDisposable Lock()
        {
            return ErrorWriter.Lock();
        }

        public bool IsLocked => ErrorWriter.IsLocked;

        private static void Write(ConsoleOutputWriter writer, char c)
        {
            writer.Write(c);
        }

        private static void Write(ConsoleOutputWriter writer, string format, params object?[] arg)
        {
            writer.Write(format, arg);
        }

        private static void WriteLine(ConsoleOutputWriter writer, string format, params object?[] arg)
        {
            writer.WriteLine(format, arg);
        }

        public void WriteOutput(char c)
        {
            WriteOutput(c.ToString());
        }

        public void WriteOutput(string text, params object?[] arg)
        {
            int position = OutputCursorHandler.Position;
            string oldText = _outputStringBuilder.ToString();
            string resolvedText = string.Format(text, arg);

            string previousHead = string.Empty;
            string previousTail = string.Empty;

            if (position <= oldText.Length)
            {
                previousHead = oldText.Substring(0, position);
            }

            if (position + resolvedText.Length <= oldText.Length)
            {
                previousTail = oldText.Substring(position + resolvedText.Length);
            }

            _outputStringBuilder.Clear();
            _outputStringBuilder.Append(previousHead);
            _outputStringBuilder.Append(resolvedText);
            _outputStringBuilder.Append(previousTail);

            MoveOutputPositionPositive(resolvedText.Length);

            needToRefresh = true;
        }

        public void WriteLineOutput(string text = "", params object?[] arg)
        {
            WriteOutput(text + Environment.NewLine, arg);
        }

        public void WriteError(char c)
        {
            WriteError(c.ToString());
        }

        public void WriteError(string text, params object?[] arg)
        {
            Write(ErrorWriter, text, arg);

            needToRefresh = true;
        }

        public void WriteLineError(string text = "", params object?[] arg)
        {
            WriteError(text + Environment.NewLine, arg);
        }

        public void WriteInput(char c)
        {
            WriteInput(c.ToString());
        }

        public void WriteInput(string text, params object?[] arg)
        {
            Write(InputWriter, text, arg);

            needToRefresh = true;
        }

        public void WriteLineInput(string text = "", params object?[] arg)
        {
            WriteInput(text + Environment.NewLine, arg);
        }

        #endregion

        #region Cursor
        public void SetInputPrefix(string prefix)
        {
            _inputPrefix = prefix;
        }

        public void SetInputPosition(int position)
        {
            _inputPosition = position;
        }

        public void MoveOutputPositionNegative(int move)
        {
            OutputCursorHandler.MovePositionNegative(move);
        }

        private void MoveOutputPositionPositive(int move)
        {
            OutputCursorHandler.MovePositionPositive(move);
        }

        private void MoveOutputPositionToOrigin()
        {
            OutputCursorHandler.MovePositionToOrigin();
        }

        private void MoveOutputPositionToTail(int textLength)
        {
            OutputCursorHandler.MovePositionToTail(textLength);
        }
        #endregion
    }
}
