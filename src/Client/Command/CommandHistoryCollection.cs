using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySharpChat.Client.Command
{
    public class CommandHistoryCollection : List<string>
    {
        private int currentPosition = -1;

        public string? CurrentCommand { get { return (currentPosition >= 0 && currentPosition < Count) ? this[currentPosition] : null; } }

        public void ResetPosition()
        {
            currentPosition = Count - 1;
        }

        public string? GetPreviousCommand()
        {
            if (currentPosition < 0)
                currentPosition = Count - 1;
            else
                currentPosition--;

            return CurrentCommand;
        }

        public string? GetNextCommand()
        {
            if (currentPosition < 0)
                currentPosition = 0;
            else
                currentPosition++;

            return CurrentCommand;
        }

        public bool TryGetPreviousCommand(out string? command)
        {
            command = GetPreviousCommand();
            return command != null;
        }

        public bool TryGetNextCommand(out string? command)
        {
            command = GetNextCommand();
            return command != null;
        }
    }
}
