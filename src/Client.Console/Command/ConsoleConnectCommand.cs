using System;
using System.Text;
using System.Threading.Tasks;
using MySharpChat.Client.Console.UI;
using MySharpChat.Core.Command;

namespace MySharpChat.Client.Console.Command
{
    internal class ConsoleConnectCommand : ConsoleCommand
    {
        public ConsoleConnectCommand(ConsoleClientImpl client, ICommand commandImpl) : base(client, commandImpl)
        {
        }

        public override bool Execute(object? data, params string[] args)
        {
            Task<bool> connectionTask = Task.Run(() => _command.Execute(data, args));

            int interation = 0;
            while (!connectionTask.Wait(TimeSpan.FromMilliseconds(500)))
            {
                interation++;
                const string prefix = "Connecting";
                const int nbDotsMax = 3;
                StringBuilder loadingText = new StringBuilder(prefix);
                int nbDots = interation % (nbDotsMax + 1);
                for (int i = 0; i < nbDots; i++)
                    loadingText.Append(".");
                for (int i = 0; i < nbDotsMax - nbDots; i++)
                    loadingText.Append(" ");
                ConsoleOutputModule outputModule = _client.UserInterfaceModule.OutputModule;
                outputModule.WriteOutput(loadingText.ToString());
                outputModule.MoveOutputPositionNegative(loadingText.Length);
            }

            bool isConnected = connectionTask.Result;
            if (isConnected)
            {
                _client.CurrentLogic = new ChatClientLogic(_client);
                _client.UserInterfaceModule.OutputModule.Clear();
            }
            else
            {
                string errorMessage = "Connection fail !";
                _client.UserInterfaceModule.OutputModule.WriteLineOutput(errorMessage);
            }

            return isConnected;
        }
    }
}
