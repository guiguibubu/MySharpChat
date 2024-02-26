using System.Collections.Generic;

namespace MySharpChat.Client.Console.Command
{
    public class CommandHistoryCollection
    {
        private int currentPosition = -1;
        private readonly List<string> commands = new List<string>();

        public string? CurrentCommand { get { return (currentPosition >= 0 && currentPosition < commands.Count) ? commands[currentPosition] : null; } }
        public string? LastCommand { get { return (currentPosition >= 0 && currentPosition < commands.Count) ? commands[commands.Count - 1] : null; } }

        public void ResetPosition()
        {
            currentPosition = commands.Count;
        }

        public void Add(string newCommand)
        {
            commands.Add(newCommand);
        }

        public string? GetPreviousCommand()
        {
            if (currentPosition < 0)
                currentPosition = commands.Count - 1;
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
            return command is not null;
        }

        public bool TryGetNextCommand(out string? command)
        {
            command = GetNextCommand();
            return command is not null;
        }
    }
}
