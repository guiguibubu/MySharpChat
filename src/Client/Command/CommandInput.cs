using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySharpChat.Client.Command
{
    public static class CommandInput
    {
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
            StringBuilder sb = new StringBuilder();
            bool readingFinished = false;
            while (!readingFinished)
            {
                int ch = Console.Read();
                if (ch == -1)
                {
                    readingFinished = true;
                }
                else
                {
                    if (ch == '\r' || ch == '\n')
                    {
                        if (ch == '\r' && Console.In.Peek() == '\n')
                            Console.Read();
                        readingFinished = true;
                    }
                    else
                    {
                        sb.Append((char)ch);
                    }
                }
            }
            return (sb.Length > 0) ? sb.ToString() : null;
        }
    }
}
