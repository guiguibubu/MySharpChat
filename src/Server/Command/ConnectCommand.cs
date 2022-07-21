using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
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

            (IEnumerable<IPAddress> ipAddressesHost, IEnumerable<IPAddress> ipAddressesNonVirtual) = SocketUtils.GetAvailableIpAdresses(serverAdress);
            data.Ip = ipAddressesHost.Intersect(ipAddressesNonVirtual).FirstOrDefault();
            if (data.Ip == null)
            {
                Console.WriteLine("No valid ip adress available");
                Console.WriteLine("Available ip adresses Host");
                foreach (IPAddress ipAddress in ipAddressesHost)
                {
                    Console.WriteLine("{0} ({1})", ipAddress, string.Join(",", ipAddress.AddressFamily));

                }
                Console.WriteLine("Available ip adresses non virtual");
                foreach (IPAddress ipAddress in ipAddressesNonVirtual)
                {
                    Console.WriteLine("{0} ({1})", ipAddress, string.Join(",", ipAddress.AddressFamily));

                }
                throw new InvalidOperationException("No valid ip adress available");
            }

            data.Port = ConnexionInfos.DEFAULT_PORT;

            return server.Connect(connexionInfos);
        }

        public string GetHelp()
        {
            return "usage: connect <ip>";
        }

        public string GetSummary()
        {
            return "Command to make server listen at a specific ip adress";
        }
    }
}
