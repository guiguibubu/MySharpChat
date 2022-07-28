using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using MySharpChat.Core.SocketModule;
using MySharpChat.Core.Utils;

namespace MySharpChat.Server.Command
{
    public class ConnectCommand : Singleton<ConnectCommand>, IServerCommand
    {
        protected ConnectCommand() { }

        public string Name { get => "Connect"; }

        public bool Execute(Server? server, params string[] args)
        {
            if(server == null)
                throw new ArgumentNullException(nameof(server));

            ConnexionInfos connexionInfos = new ConnexionInfos();
            string? serverAdress = args.Length > 0 ? args[0] : null;
            ConnexionInfos.Data data = connexionInfos.Local!;

            LockTextWriter writer = server.OutputWriter;

            (IEnumerable<IPAddress> ipAddressesHost, IEnumerable<IPAddress> ipAddressesNonVirtual) = SocketUtils.GetAvailableIpAdresses(serverAdress);
            data.Ip = ipAddressesHost.Intersect(ipAddressesNonVirtual).FirstOrDefault();
            if (data.Ip == null)
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
                throw new InvalidOperationException("No valid ip adress available");
            }

            data.Port = ConnexionInfos.DEFAULT_PORT;

            return server.Connect(connexionInfos);
        }

        public bool Execute(object? data, params string[] args)
        {
            return (this as IServerCommand).Execute(data, args);
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
