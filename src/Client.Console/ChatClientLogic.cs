using MySharpChat.Client.Command;
using MySharpChat.Client.Console.Command;
using MySharpChat.Core.Command;

namespace MySharpChat.Client.Console
{
    internal class ChatClientLogic : IClientLogic
    {
        private readonly CommandManager commandManager = new CommandManager();
        private readonly CommandParser commandParser;
        public CommandParser CommandParser => commandParser;

        public string Prefix => string.Format("{0}@{1}> ", _client.LocalUser.Username, ((ClientNetworkModule)_client.NetworkModule).ServerUri!.Host);

        private readonly ConsoleClientImpl _client;

        public ChatClientLogic(ConsoleClientImpl client)
        {
            _client = client;

            commandManager.AddCommand(SendCommand.Instance);
            commandManager.AddCommand(UsernameCommand.Instance);
            commandManager.AddCommand(UserCommand.Instance);
            commandManager.AddCommand(new ConsoleDisconnectCommand(client, DisconnectCommand.Instance));
            commandManager.AddCommand(QuitCommand.Instance);
            commandManager.AddCommand(ExitCommand.Instance);

            commandParser = new CommandParser(commandManager);
        }
    }
}
