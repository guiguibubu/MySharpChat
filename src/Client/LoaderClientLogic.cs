using MySharpChat.Client.Command;
using MySharpChat.Core.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySharpChat.Client
{
    public class LoaderClientLogic : IClientLogic
    {
        private readonly CommandManager commandManager = new CommandManager();
        private readonly CommandParser commandParser;
        public CommandParser CommandParser => commandParser;

        public string Prefix => string.Format("{0}> ", Environment.UserName);

        public LoaderClientLogic()
        {
            commandManager.AddCommand(QuitCommand.Instance);
            commandManager.AddCommand(ExitCommand.Instance);
            commandManager.AddCommand(ConnectCommand.Instance);

            commandParser = new CommandParser(commandManager);
        }
    }
}
