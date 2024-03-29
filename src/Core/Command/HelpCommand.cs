﻿using MySharpChat.Core.Utils;
using System;
using System.Linq;

namespace MySharpChat.Core.Command
{
    public class HelpCommand : ICommand<LockTextWriter>
    {
        private readonly CommandManager _commandManager;

        public HelpCommand(CommandManager commandManager)
        {
            _commandManager = commandManager;
        }

        public string Name => "Help";

        public bool Execute(LockTextWriter? writer, params string[] args)
        {
            if(writer is null)
                throw new ArgumentNullException(nameof(writer));

            string? commandName = args.Length > 0 ? args[0] : null;

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

                if (command is null)
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
