using MySharpChat.Core.Utils;
using System;
using System.Linq;

namespace MySharpChat.Core.Command
{
    public class HelpCommand : IAsyncMachineCommand
    {
        private readonly CommandManager _commandManager;

        public HelpCommand(CommandManager commandManager)
        {
            _commandManager = commandManager;
        }

        public string Name { get => "Help"; }

        public bool Execute(IAsyncMachine? asyncMachine, params string[] args)
        {
            if(asyncMachine == null)
                throw new ArgumentNullException(nameof(asyncMachine));

            string? commandName = args.Length > 0 ? args[0] : null;

            LockTextWriter writer = asyncMachine.OutputWriter;

            if (string.IsNullOrEmpty(commandName))
            {
                foreach (ICommand command in _commandManager.GetCommands())
                {
                    writer.Write("{0} : ", command.Name);
                    string helpMsg;
                    try
                    {
                        helpMsg = command.GetSummary();
                    }
                    catch (NotImplementedException)
                    {
                        helpMsg = "No help for this command";
                    }
                    writer.WriteLine(helpMsg);
                }
                writer.WriteLine();
                writer.WriteLine("To have help on a specific command : help <command>");
            }
            else
            {
                ICommand? command = _commandManager.GetCommands().FirstOrDefault(c => string.Equals(c.Name, commandName, StringComparison.InvariantCultureIgnoreCase));

                if (command == null)
                {
                    writer.Write("Unknown command \"{0}\"", commandName);
                }
                else
                {
                    writer.WriteLine("{0} :", command.Name);
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
                    writer.WriteLine(helpMsg);
                }
            }
            return true;
        }

        public bool Execute(object? data, params string[] args)
        {
            return (this as IAsyncMachineCommand).Execute(data, args);
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
