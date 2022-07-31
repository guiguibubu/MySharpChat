using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using MySharpChat.Core.SocketModule;
using MySharpChat.Core.Utils;
using MySharpChat.Core.Utils.Logger;

namespace MySharpChat.Server.Command
{
    public class ConnectCommand : Singleton<ConnectCommand>, IServerCommand
    {
        private static readonly Logger logger = Logger.Factory.GetLogger<Server>();

        protected ConnectCommand() { }

        public string Name { get => "Connect"; }

        public bool Execute(IServerImpl? server, params string[] args)
        {
            if(server == null)
                throw new ArgumentNullException(nameof(server));

            ConnexionInfos connexionInfos = new ConnexionInfos();
            string? serverAdress = args.Length > 0 ? args[0] : null;
            ConnexionInfos.Data data = connexionInfos.Local!;

            (IEnumerable<IPAddress> ipAddressesHost, IEnumerable<IPAddress> ipAddressesNonVirtual) = SocketUtils.GetAvailableIpAdresses(serverAdress);
            data.Ip = ipAddressesHost.Intersect(ipAddressesNonVirtual).FirstOrDefault();
            if (data.Ip == null)
            {
                logger.LogError("No valid ip adress available");
                logger.LogError("Available ip adresses Host");
                foreach (IPAddress ipAddress in ipAddressesHost)
                {
                    logger.LogError("{0} ({1})", ipAddress, string.Join(",", ipAddress.AddressFamily));

                }
                logger.LogError("Available ip adresses non virtual");
                foreach (IPAddress ipAddress in ipAddressesNonVirtual)
                {
                    logger.LogError("{0} ({1})", ipAddress, string.Join(",", ipAddress.AddressFamily));

                }
                throw new InvalidOperationException("No valid ip adress available");
            }

            data.Port = ConnexionInfos.DEFAULT_PORT;

            return server.NetworkModule.Connect(connexionInfos);
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
