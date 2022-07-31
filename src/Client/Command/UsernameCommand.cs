using MySharpChat.Core.Command;
using MySharpChat.Core.Packet;
using MySharpChat.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySharpChat.Client.Command
{
    internal class UsernameCommand : Singleton<UsernameCommand>, IClientCommand
    {
        public string Name => "Username";

        protected UsernameCommand() { }

        public bool Execute(IClientImpl? client, params string[] args)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            string? newUsername = args.Length > 0 ? args[0] : null;
            if (!string.IsNullOrEmpty(newUsername))
            {
                ClientInitialisationPacket initPacket = new ClientInitialisationPacket(client.ClientId, newUsername);
                PacketWrapper packet = new PacketWrapper(client.ClientId, initPacket);
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
            return "usage: username <new_username>";
        }

        public string GetSummary()
        {
            return "Command to change your username.";
        }
    }
}
