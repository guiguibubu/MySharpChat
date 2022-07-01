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
#if DEBUG
            data.Hostname = serverAdress ?? "localhost";
#else
            data.Hostname = serverAdress ?? Dns.GetHostName();
#endif

            List<NetworkInterface> networkInterfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(ni => ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet) //WiFI or Ethernet
                .Where(ni => ni.GetIPProperties().GatewayAddresses.FirstOrDefault() != null) //Virtual (like VirtualBox) network interfaces does not have Gateway address
                .ToList();

            List<IPAddress> ipAddressesNonVirtual = networkInterfaces!.Select(ni => ni.GetIPProperties()).SelectMany(ipprop => ipprop.UnicastAddresses).Select(uniAddr => uniAddr.Address).ToList();

            IPHostEntry ipHostInfo = Dns.GetHostEntry(data.Hostname);
            IPAddress[] ipAddresses = ipHostInfo.AddressList;
#if DEBUG
            Console.WriteLine("Available ip adresses");
            foreach(IPAddress ipAddress in ipAddresses)
            {
                Console.WriteLine("{0} ({1})", ipAddress, string.Join(",", ipAddress.AddressFamily));

            }
            Console.WriteLine("Available ip adresses non virtual");
            foreach (IPAddress ipAddress in ipAddressesNonVirtual)
            {
                Console.WriteLine("{0} ({1})", ipAddress, string.Join(",", ipAddress.AddressFamily));

            }
#endif
            data.Ip = ipAddresses.First(a => a.AddressFamily == AddressFamily.InterNetwork);
            data.Port = ConnexionInfos.DEFAULT_PORT;

            return server.Connect(connexionInfos);
        }

        public string GetHelp()
        {
            throw new NotImplementedException();
        }
    }
}
