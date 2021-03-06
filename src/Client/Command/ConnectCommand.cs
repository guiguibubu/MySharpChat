using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using MySharpChat.Core.SocketModule;
using MySharpChat.Core.Utils;

namespace MySharpChat.Client.Command
{
    internal class ConnectCommand : Singleton<ConnectCommand>, IClientCommand
    {
        protected ConnectCommand() { }

        public string Name { get => "Connect"; }

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
                using (LockTextWriter writer = client.OutputWriter)
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
            client.CurrentLogic = new ChatClientLogic(client.NetworkModule.LocalEndPoint);
            return isConnected;
        }

        public bool Execute(object? data, params string[] args)
        {
            return (this as IClientCommand).Execute(data, args);
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
