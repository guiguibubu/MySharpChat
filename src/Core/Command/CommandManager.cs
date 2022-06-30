using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using MySharpChat.Core.Utils;

namespace MySharpChat.Core.Command
{
    public class CommandManager : Singleton<CommandManager>
    {
        protected CommandManager() { }

        private readonly Dictionary<string, ICommand> _commands = new Dictionary<string, ICommand>(CommandComparer.NameComparer);

        public ICommand? GetCommand(string? name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            bool commandExist = _commands.TryGetValue(name, out ICommand? command);
            return commandExist ? command : null;
        }

        public T? GetCommand<T>(string? name) where T : ICommand
        {
            return (T?)GetCommand(name);
        }

        public void AddCommand(ICommand? command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (_commands.Values.Contains(command, CommandComparer.Instance))
                throw new ArgumentException($"This command was already registered : {command.Name}");

            _commands.Add(command.Name, command);
        }

        public ICommand? this[string name]
        {
            get
            {
                return GetCommand(name);
            }
        }
    }
}
