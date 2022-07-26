using MySharpChat.Core.Utils;

namespace MySharpChat.Client.Command
{
    internal class DisconnectCommand : Singleton<DisconnectCommand>, IClientCommand
    {
        protected DisconnectCommand() { }

        public string Name { get => "Disconnect"; }

        public bool Execute(IClientImpl client, params string[] args)
        {
            client.Disconnect();
            return true;
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
