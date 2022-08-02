using MySharpChat.Core.Packet;
using MySharpChat.Core.SocketModule;
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

        private Socket? m_socket = null;

        public ServerNetworkModule()
        { }

        public string LocalEndPoint
        {
            get
            {
                if (m_socket != null && m_socket.LocalEndPoint != null)
                    return m_socket.LocalEndPoint.ToString() ?? string.Empty;
                else
                    return string.Empty;
            }
        }

        public string RemoteEndPoint
        {
            get
            {
                if (m_socket != null && m_socket.RemoteEndPoint != null)
                    return m_socket.RemoteEndPoint.ToString() ?? string.Empty;
                else
                    return string.Empty;
            }
        }

        public bool HasDataAvailable => m_socket != null && m_socket.Available > 0;

        public bool IsConnectionPending => m_socket != null && SocketUtils.IsConnectionPending(m_socket);

        public bool Connect(IPEndPoint remoteEP, int timeoutMs = Timeout.Infinite)
        {
            throw new NotImplementedException("Use Connect(ConnexionInfos connexionInfos) instead");
        }

        public bool Connect(ConnexionInfos connexionInfos)
        {
            ConnexionInfos.Data? connexionData = connexionInfos.Local;
            if (connexionData == null)
                throw new ArgumentException(nameof(connexionInfos.Local));

            IPEndPoint localEndPoint = SocketUtils.CreateEndPoint(connexionData);

            // Create a TCP/IP socket.  
            m_socket = SocketUtils.CreateSocket(connexionData);

            // Bind the socket to the local endpoint and listen for incoming connections. 
            m_socket.Bind(localEndPoint);

            m_socket.Listen(100);

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
            if (m_socket != null)
            {
                SocketUtils.ShutdownSocket(m_socket);
            }
        }

        public Socket Accept()
        {
            return m_socket!.Accept();
        }

        public bool IsConnected()
        {
            return m_socket != null && SocketUtils.IsConnected(m_socket);
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
