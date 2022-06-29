using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MySharpChat.Core.Utils;

namespace MySharpChat.Core.Command
{
    public class CommandParser : Singleton<CommandParser>, IParser<ICommand>
    {
        private const string COMMAND_PREFIX = "command";

        protected CommandParser() { }

        public ICommand? Parse(string? text, out string[] args)
        {
            args = new string[0];

            if (text == null)
                return null;
            if (!text.StartsWith(COMMAND_PREFIX + " ", StringComparison.InvariantCultureIgnoreCase))
                return null;

            string[] commandTokens = text.Substring(COMMAND_PREFIX.Length + 1).Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            
            if(!commandTokens.Any())
                return null;

            string commandName = commandTokens[0];
            args = new ArraySegment<string>(commandTokens, 1, commandTokens.Length - 1).ToArray();
            return CommandManager.Instance?.GetCommand(commandName);
        }

        public ICommand? Parse(string? text)
        {
            return Parse(text, out _);
        }

        public bool TryParse(string? text, out string[] args, out ICommand? command)
        {
            command = Parse(text, out args);
            return command != null;
        }

        public bool TryParse(string? text, out ICommand? parsedObject)
        {
            return TryParse(text, out _, out parsedObject);
        }
    }
}
