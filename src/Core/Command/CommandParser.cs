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

        public T? Parse<T>(string? text, out string[] args) where T : ICommand
        {
            return (T?)Parse(text, out args);
        }

        public T? Parse<T>(string? text) where T : ICommand
        {
            return Parse<T>(text, out _);
        }

        public bool TryParse(string? text, out string[] args, out ICommand? command)
        {
            command = Parse(text, out args);
            return command is not null;
        }

        public bool TryParse(string? text, out ICommand? parsedObject)
        {
            return TryParse(text, out _, out parsedObject);
        }

        public bool TryParse<T>(string? text, out string[] args, out T? command) where T : class, ICommand
        {
            try
            {
                command = (T?)Parse(text, out args);
            }
            catch (InvalidCastException)
            {
                args = new string[0];
                command = null;
            }
            return command is not null;
        }

        public bool TryParse<T>(string? text, out T? command) where T : class, ICommand
        {
            return TryParse(text, out _, out command);
        }
    }
}
