using System;
using MySharpChat.Core.Command;
using MySharpChat.Core.Utils;

namespace MySharpChat.Client.Command
{
    public class SendCommand : Singleton<SendCommand>, IClientCommand
    {
        protected SendCommand() { }

        public string Name => "Send";

        public bool Execute(Client client, params string[] args)
        {
            string? text = args.Length > 0 ? args[0] : null;
            client.Send(text);

            return true;
        }

        public string GetHelp()
        {
            return "usage: send <text>";
        }

        public string GetSummary()
        {
            return "Command to send messages";
        }
    }
}
