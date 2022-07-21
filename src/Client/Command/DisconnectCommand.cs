using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using MySharpChat.Core.Command;
using MySharpChat.Core.SocketModule;
using MySharpChat.Core.Utils;

namespace MySharpChat.Client.Command
{
    public class DisconnectCommand : Singleton<DisconnectCommand>, IClientCommand
    {
        protected DisconnectCommand() { }

        public string Name { get => "Disconnect"; }

        public bool Execute(AsynchronousClient client, params string[] args)
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
