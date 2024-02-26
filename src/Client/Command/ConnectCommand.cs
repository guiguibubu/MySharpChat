using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using MySharpChat.Core.Command;
using MySharpChat.Core.NetworkModule;
using MySharpChat.Core.Utils;

namespace MySharpChat.Client.Command
{
    public class ConnectCommand : Singleton<ConnectCommand>, IClientCommand
    {
        protected ConnectCommand() { }

        public string Name => "Connect";

        public bool Execute(IClientImpl? client, params string[] args)
        {
            if (client is null)
                throw new ArgumentNullException(nameof(client));

            ConnexionInfos connexionInfos = new ConnexionInfos();
            string? serverAdress = args.Length > 0 ? args[0] : null;
            ConnexionInfos.Data data = connexionInfos.Remote!;

            (IEnumerable<IPAddress> ipAddressesHost, IEnumerable<IPAddress> ipAddressesNonVirtual) = NetworkUtils.GetAvailableIpAdresses(serverAdress);
            data.Ip = ipAddressesHost.Intersect(ipAddressesNonVirtual).FirstOrDefault();
            if (data.Ip is null)
            {
                StringWriter writer = new StringWriter();
                writer.WriteLine("No valid ip adress available");
                writer.WriteLine("Available ip adresses Host");
                foreach (IPAddress ipAddress in ipAddressesHost)
                {
                    writer.WriteLine("{0} ({1})", ipAddress, string.Join(",", ipAddress.AddressFamily));

                }
                writer.WriteLine("Available ip adresses non virtual");
                foreach (IPAddress ipAddress in ipAddressesNonVirtual)
                {
                    writer.WriteLine("{0} ({1})", ipAddress, string.Join(",", ipAddress.AddressFamily));

                }
                throw new InvalidOperationException(writer.ToString());
            }

            data.Port = ConnexionInfos.DEFAULT_PORT;

            return client.NetworkModule.Connect(connexionInfos);
        }

        public string GetHelp()
        {
            return "usage: connect <ip>";
        }

        public string GetSummary()
        {
            return "Command to connect to server.";
        }
    }
}
