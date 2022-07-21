using MySharpChat.Core.Utils;
using System;
using System.Linq;

namespace MySharpChat.Core.Command
{
    public class HelpCommand : ICommand
    {
        private readonly CommandManager _commandManager;

        public HelpCommand(CommandManager commandManager)
        {
            _commandManager = commandManager;
        }

        public string Name { get => "Help"; }

        public bool Execute(IAsyncMachine? asyncMachine, params string[] args)
        {
            string? commandName = args.Length > 0 ? args[0] : null;

            if (string.IsNullOrEmpty(commandName))
            {
                foreach (ICommand command in _commandManager.GetCommands())
                {
                    Console.Write("{0} : ", command.Name);
                    string helpMsg;
                    try
                    {
                        helpMsg = command.GetSummary();
                    }
                    catch (NotImplementedException)
                    {
                        helpMsg = "No help for this command";
                    }
                    Console.WriteLine(helpMsg);
                }
                Console.WriteLine();
                Console.WriteLine("To have help on a specific command : help <command>");
            }
            else
            {
                ICommand? command = _commandManager.GetCommands().FirstOrDefault(c => string.Equals(c.Name, commandName, StringComparison.InvariantCultureIgnoreCase));

                if (command == null)
                {
                    Console.Write("Unknown command \"{0}\"", commandName);
                }
                else
                {
                    Console.WriteLine("{0} :", command.Name);
                    string helpMsg;
                    try
                    {
                        helpMsg = command.GetSummary();
                        helpMsg += Environment.NewLine;
                        helpMsg += command.GetHelp();
                    }
                    catch (NotImplementedException)
                    {
                        helpMsg = "No help for this command";
                    }
                    Console.WriteLine(helpMsg);
                }
            }
            return true;
        }

        public bool Execute()
        {
            return Execute(null);
        }

        public string GetHelp()
        {
            return string.Join(Environment.NewLine,"usage:", "\"help\" to list all available commands", "\"help <command>\" to have help for specific command");
        }

        public string GetSummary()
        {
            return "Command to have help";
        }
    }
}
