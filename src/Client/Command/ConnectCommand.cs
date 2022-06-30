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
    public class ConnectCommand : Singleton<ConnectCommand>, ICommand
    {
        protected ConnectCommand() { }

        public string Name { get => "Connect"; }

        public bool Execute(IAsyncMachine? asyncMachine, params string[] args)
        {
            if (asyncMachine == null)
                throw new ArgumentNullException(nameof(asyncMachine));

            if (asyncMachine is AsynchronousClient client)
            {
                return Execute(client, args);
            }
            else
            {
                throw new ArgumentException(string.Format("{0} must be a {1}", nameof(asyncMachine), typeof(AsynchronousClient)));
            }
        }

        private bool Execute(AsynchronousClient client, params string[] args)
        {
            ConnexionInfos connexionInfos = new ConnexionInfos();
            string? serverAdress = args.Length > 0 ? args[0] : null;
#if DEBUG
            connexionInfos.Remote!.Hostname = serverAdress ?? "localhost";
#else
            connexionInfos.Remote!.Hostname = serverAdress ?? Dns.GetHostName();
#endif

            IPHostEntry ipHostInfo = Dns.GetHostEntry(connexionInfos.Remote.Hostname);
            connexionInfos.Remote.Ip = ipHostInfo.AddressList.First(a => a.AddressFamily == AddressFamily.InterNetwork);
            connexionInfos.Remote.Port = ConnexionInfos.DEFAULT_PORT;

            return client.Connect(connexionInfos);
        }

        public string GetHelp()
        {
            throw new NotImplementedException();
        }
    }
}
