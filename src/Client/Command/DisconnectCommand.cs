using MySharpChat.Core.Utils;

namespace MySharpChat.Client.Command
{
    public class DisconnectCommand : Singleton<DisconnectCommand>, IClientCommand
    {
        protected DisconnectCommand() { }

        public string Name { get => "Disconnect"; }

        public bool Execute(Client client, params string[] args)
        {
            client.Disconnect(null);
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
