using MySharpChat.Core.Command;
using System;

namespace MySharpChat.Client.Console.Command
{
    internal class ConsoleCommand : ICommand
    {
        protected readonly ConsoleClientImpl _client;
        protected readonly ICommand _command;

        public string Name => _command.Name;

        protected ConsoleCommand(ConsoleClientImpl client, ICommand commandImpl)
        {
            _client = client;
            _command = commandImpl;
        }

        public virtual bool Execute(object? data, params string[] args)
        {
            return _command.Execute(data, args);
        }

        public string GetHelp()
        {
            return _command.GetHelp();
        }

        public string GetSummary()
        {
            return _command.GetSummary();
        }

        public bool UnderlyingCommandIs(Type type)
        {
            return _command.GetType().IsAssignableTo(type);
        }
    }
}
