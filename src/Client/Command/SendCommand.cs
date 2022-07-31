using System;
using MySharpChat.Core;
using MySharpChat.Core.Command;
using MySharpChat.Core.Packet;
using MySharpChat.Core.Utils;

namespace MySharpChat.Client.Command
{
    internal class SendCommand : Singleton<SendCommand>, IClientCommand
    {
        protected SendCommand() { }

        public string Name => "Send";

        public bool Execute(IClientImpl? client, params string[] args)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            if (client.ClientId == Guid.Empty)
                throw new ConnectionNotInitializedException();

            string? text = args.Length > 0 ? args[0] : null;
            if (!string.IsNullOrEmpty(text))
            {
                ChatPacket chatPacket = new ChatPacket(text);
                PacketWrapper packet = new PacketWrapper(client.ClientId, chatPacket);
                client.NetworkModule.Send(packet);
            }
            else
            {
                HelpCommand helpCommand = client.CurrentLogic.CommandParser.GetHelpCommand();
                helpCommand.Execute(client.UserInterfaceModule.OutputWriter, Name);
            }
            return true;
        }

        public bool Execute(object? data, params string[] args)
        {
            return (this as IClientCommand).Execute(data, args);
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
