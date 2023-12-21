using System.IO;
using MySharpChat.Core.Utils;

namespace MySharpChat.Core.Console
{
    public class ConsoleOutputWriter : LockTextWriter
    {
        private readonly StringWriter _outputCache;

        public ConsoleOutputWriter(TextWriter output) : base(output)
        {
            _outputCache = (StringWriter)_output;
        }

        public ConsoleOutputWriter() : this(new StringWriter()) { }

        public override void Flush()
        {
            string text = _outputCache.ToString();
            System.Console.Write(text);
            _output.Flush();
        }
    }
}
