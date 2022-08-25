using MySharpChat.Core.Packet;
using MySharpChat.Core.SocketModule;
using MySharpChat.Core.Utils;
using MySharpChat.Core.Utils.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MySharpChat.Server
{
    internal class ConsoleServerImpl : IServerImpl
    {
        private static readonly Logger logger = Logger.Factory.GetLogger<ConsoleServerImpl>();

        private readonly ServerNetworkModule networkModule;
        public INetworkModule NetworkModule => networkModule;

        public Guid ServerId { get; private set; } = Guid.NewGuid();

        public ChatRoom ChatRoom { get; private set; }

        private readonly HttpServer httpServer;
        public ConsoleServerImpl()
        {
            networkModule = new ServerNetworkModule();
            ChatRoom = new ChatRoom(ServerId);
            httpServer = new HttpServer();
        }

        public void Run(Server server)
        {
            // Start an asynchronous socket to listen for connections.  
            logger.LogDebug("Waiting for a connection...");

            while (!networkModule.IsConnectionPending)
            {
                Thread.Sleep(1000);
            }

            TcpClient tcpClient = networkModule.Accept();
            tcpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            tcpClient.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, (int)TimeSpan.FromHours(2).TotalMilliseconds);
            tcpClient.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, (int)TimeSpan.FromSeconds(1).TotalMilliseconds);
            tcpClient.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, 10);
            LaunchSession(tcpClient);
        }

        public void Start()
        {
            httpServer.Start(System.Net.IPEndPoint.Parse(networkModule.LocalEndPoint));
        }

        public void Stop()
        {
            networkModule.Disconnect();
        }

        public bool Connect(ConnexionInfos connexionInfos)
        {
            return networkModule.Connect(connexionInfos);
        }

        private void LaunchSession(TcpClient tcpClient)
        {
            ChatRoom.LaunchSession(tcpClient);
        }
    }
}
