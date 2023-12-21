using MySharpChat.Client.Command;
using MySharpChat.Client.Console.Command;
using MySharpChat.Core.Command;

namespace MySharpChat.Client.Console
{
    internal class LoaderClientLogic : IClientLogic
    {
        private readonly CommandManager commandManager = new CommandManager();
        private readonly CommandParser commandParser;
        public CommandParser CommandParser => commandParser;

        private readonly ConsoleClientImpl _client;
        public string Prefix => string.Format("{0}> ", _client.LocalUser.Username);

        public LoaderClientLogic(ConsoleClientImpl client)
        {
            _client = client;

            commandManager.AddCommand(QuitCommand.Instance);
            commandManager.AddCommand(ExitCommand.Instance);
            commandManager.AddCommand(new ConsoleConnectCommand(client, ConnectCommand.Instance));

            commandParser = new CommandParser(commandManager);
        }
    }
}
