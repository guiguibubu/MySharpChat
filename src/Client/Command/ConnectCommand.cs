using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using MySharpChat.Core.Command;
using MySharpChat.Core.SocketModule;
using MySharpChat.Core.Utils;

namespace MySharpChat.Client.Command
{
    public class ConnectCommand : Singleton<ConnectCommand>, IClientCommand
    {
        protected ConnectCommand() { }

        public string Name => "Connect";

        public bool Execute(IClientImpl? client, params string[] args)
        {
            if(client == null)
                throw new ArgumentNullException(nameof(client));

            ConnexionInfos connexionInfos = new ConnexionInfos();
            string? serverAdress = args.Length > 0 ? args[0] : null;
            ConnexionInfos.Data data = connexionInfos.Remote!;

            (IEnumerable<IPAddress> ipAddressesHost, IEnumerable<IPAddress> ipAddressesNonVirtual) = SocketUtils.GetAvailableIpAdresses(serverAdress);
            data.Ip = ipAddressesHost.Intersect(ipAddressesNonVirtual).FirstOrDefault();
            if (data.Ip == null)
            {
                LockTextWriter writer = client.UserInterfaceModule.OutputWriter;
                using (writer.Lock())
                {
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
                }
                throw new InvalidOperationException("No valid ip adress available");
            }

            data.Port = ConnexionInfos.DEFAULT_PORT;

            bool isConnected = client.NetworkModule.Connect(connexionInfos);
            if(isConnected)
                client.CurrentLogic = new ChatClientLogic(client);
            return isConnected;
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
