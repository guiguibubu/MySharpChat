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

namespace MySharpChat.Server.Command
{
    public class ConnectCommand : Singleton<ConnectCommand>, IServerCommand
    {
        protected ConnectCommand() { }

        public string Name { get => "Connect"; }

        public bool Execute(AsynchronousServer server, params string[] args)
        {
            ConnexionInfos connexionInfos = new ConnexionInfos();
            string? serverAdress = args.Length > 0 ? args[0] : null;
            ConnexionInfos.Data data = connexionInfos.Local!;
#if DEBUG
            data.Hostname = serverAdress ?? "localhost";
#else
            data.Hostname = serverAdress ?? Dns.GetHostName();
#endif

            IPHostEntry ipHostInfo = Dns.GetHostEntry(data.Hostname);
            data.Ip = ipHostInfo.AddressList.First(a => a.AddressFamily == AddressFamily.InterNetwork);
            data.Port = ConnexionInfos.DEFAULT_PORT;

            return server.Connect(connexionInfos);
        }

        public string GetHelp()
        {
            throw new NotImplementedException();
        }
    }
}
