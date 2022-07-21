using System;
using System.Linq;

using MySharpChat.Core.Utils;

namespace MySharpChat.Core.Command
{
    public class CommandParser : IParser<ICommand>
    {
        private readonly CommandManager _commandManager;

        public CommandParser(CommandManager commandManager) {
            _commandManager = commandManager;
        }

        public HelpCommand GetHelpCommand()
        {
            return _commandManager.GetHelpCommand();
        }

        public ICommand? Parse(string? text, out string[] args)
        {
            args = new string[0];

            if (string.IsNullOrEmpty(text))
                return null;

            string[] commandTokens = text.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            
            if(!commandTokens.Any())
                return null;

            string commandName = commandTokens[0];
            args = new ArraySegment<string>(commandTokens, 1, commandTokens.Length - 1).ToArray();
            return _commandManager.GetCommand(commandName);
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
