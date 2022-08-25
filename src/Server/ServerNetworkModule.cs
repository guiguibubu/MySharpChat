using MySharpChat.Core.Packet;
using MySharpChat.Core.NetworkModule;
using MySharpChat.Core.Utils;
using MySharpChat.Core.Utils.Logger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MySharpChat.Server
{
    public class ServerNetworkModule : INetworkModule
    {
        private static readonly Logger logger = Logger.Factory.GetLogger<ServerNetworkModule>();

        private TcpListener? tcpListener = null;

        public ServerNetworkModule()
        { }

        public string LocalEndPoint
        {
            get
            {
                if (tcpListener != null && tcpListener.LocalEndpoint != null)
                    return tcpListener.LocalEndpoint.ToString() ?? string.Empty;
                else
                    return string.Empty;
            }
        }

        public string RemoteEndPoint => string.Empty;

        public bool HasDataAvailable => false;

        public bool IsConnectionPending => tcpListener != null && NetworkUtils.IsConnectionPending(tcpListener);

        public bool Connect(IPEndPoint remoteEP, int timeoutMs = Timeout.Infinite)
        {
            throw new NotImplementedException("Use Connect(ConnexionInfos connexionInfos) instead");
        }

        public bool Connect(ConnexionInfos connexionInfos)
        {
            ConnexionInfos.Data? connexionData = connexionInfos.Local;
            if (connexionData == null)
                throw new ArgumentException(nameof(connexionInfos.Local));

            IPEndPoint localEndPoint = NetworkUtils.CreateEndPoint(connexionData);

            // Create a TCP/IP socket.  
            tcpListener = new TcpListener(localEndPoint);

            // Bind the socket to the local endpoint and listen for incoming connections. 
            tcpListener.Start(100);

            logger.LogInfo(string.Format("Listenning at {0} : {1}:{2}", connexionData.Hostname, connexionData.Ip, connexionData.Port));

            return true;
        }

        public Task<bool> ConnectAsync(ConnexionInfos connexionInfos)
        {
            return Task.Run(() => Connect(connexionInfos));
        }

        public Task<bool> ConnectAsync(IPEndPoint remoteEP, int timeoutMs = Timeout.Infinite)
        {
            throw new NotImplementedException("Use ConnectAsync(ConnexionInfos connexionInfos) instead");
        }

        public void Disconnect()
        {
            if (tcpListener != null)
            {
                tcpListener.Stop();
            }
        }

        public TcpClient Accept()
        {
            return tcpListener!.AcceptTcpClient();
        }

        public bool IsConnected()
        {
            return tcpListener != null;
        }

        public void Send(PacketWrapper? packet)
        {
            throw new NotImplementedException("Server should not be able to send data");
        }

        public List<PacketWrapper> Read(TimeSpan timeoutSpan)
        {
            throw new NotImplementedException("Server should not be able to read data");
        }
    }
}
