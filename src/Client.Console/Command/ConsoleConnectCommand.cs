using MySharpChat.Client.Console.UI;
using MySharpChat.Core.Command;
using MySharpChat.Core.Utils;
using System;
using System.Text;
using System.Threading.Tasks;

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
                IUserInputCursorHandler cursorHandler = _client.UserInterfaceModule.CursorHandler;
                LockTextWriter writer = _client.UserInterfaceModule.OutputWriter;
                writer.Write(loadingText);
                cursorHandler.MovePositionNegative(loadingText.Length, CursorUpdateMode.GraphicalOnly);
            }

            bool isConnected = connectionTask.Result;
            if (isConnected)
            {
                _client.CurrentLogic = new ChatClientLogic(_client);
                System.Console.Clear();
            }
            else
                _client.UserInterfaceModule.OutputWriter.WriteLine("Connection fail !");
            return isConnected;
        }
    }
}
