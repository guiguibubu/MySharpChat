using MySharpChat.Client.Command;
using MySharpChat.Core.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MySharpChat.Client
{
    internal class ChatClientLogic : IClientLogic
    {
        private readonly CommandManager commandManager = new CommandManager();
        private readonly CommandParser commandParser;
        public CommandParser CommandParser => commandParser;

        public string Prefix => string.Format("{0}@{1}> ", Environment.UserName, _endPoint);

        private readonly string _endPoint;

        public ChatClientLogic(string endPoint)
        {
            _endPoint = endPoint;

            commandManager.AddCommand(SendCommand.Instance);
            commandManager.AddCommand(DisconnectCommand.Instance);
            commandManager.AddCommand(QuitCommand.Instance);
            commandManager.AddCommand(ExitCommand.Instance);

            commandParser = new CommandParser(commandManager);
        }
    }
}
