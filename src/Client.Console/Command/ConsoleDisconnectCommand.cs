using MySharpChat.Core.Command;

namespace MySharpChat.Client.Console.Command
{
    internal class ConsoleDisconnectCommand : ConsoleCommand
    {
        public ConsoleDisconnectCommand(ConsoleClientImpl client, ICommand commandImpl) : base(client, commandImpl)
        {

        }

        public override bool Execute(object? data, params string[] args)
        {
            bool commandSuccess = _command.Execute(data, args);
            if (commandSuccess)
                _client.CurrentLogic = new LoaderClientLogic(_client);
            return commandSuccess;
        }
    }
}
