using MySharpChat.Core.Utils;
using System;

namespace MySharpChat.Client.Command
{
    internal class DisconnectCommand : Singleton<DisconnectCommand>, IClientCommand
    {
        protected DisconnectCommand() { }

        public string Name => "Disconnect";

        public bool Execute(IClientImpl? client, params string[] args)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            client.NetworkModule.Disconnect();
            client.CurrentLogic = new LoaderClientLogic(client);
            return true;
        }

        public bool Execute(object? data, params string[] args)
        {
            return (this as IClientCommand).Execute(data, args);
        }

        public string GetHelp()
        {
            return "usage: disconnect";
        }

        public string GetSummary()
        {
            return "Command to disconnect from server.";
        }
    }
}
