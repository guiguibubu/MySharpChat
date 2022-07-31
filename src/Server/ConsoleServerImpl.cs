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

        private readonly List<Socket> m_connectedSockets = new List<Socket>();

        public string LocalEndPoint => networkModule.LocalEndPoint;

        public string RemoteEndPoint => networkModule.RemoteEndPoint;

        public ConsoleServerImpl()
        {
            networkModule = new ServerNetworkModule();
        }

        public void Run(Server server)
        {
            // Start an asynchronous socket to listen for connections.  
            logger.LogDebug("Waiting for a connection...");

            while (!networkModule.IsConnectionPending)
            {
                Thread.Sleep(1000);
            }

            Socket connectedSocket = networkModule.Accept();

            m_connectedSockets.Add(connectedSocket);

            LaunchSession(connectedSocket);
        }

        public void Stop()
        {
            networkModule.Disconnect();
        }

        public bool Connect(ConnexionInfos connexionInfos)
        {
            return networkModule.Connect(connexionInfos);
        }

        private void LaunchSession(Socket? socket)
        {
            if (socket == null)
                throw new ArgumentNullException(nameof(socket));

            Task.Run(() =>
                {
                    ChatSession session = new ChatSession(socket);
                    session.OnSessionFinishedCallback += OnSessionFinished;
                    session.OnBroadcastCallback += Broadcast;
                    session.Start();
                }
            );
        }

        private void OnSessionFinished(ChatSession session)
        {
            m_connectedSockets.Remove(session.NetworkModule.Socket);
        }

        private void Broadcast(ChatSession origin, string text)
        {
            IEnumerable<Socket> socketToBroadcast = m_connectedSockets.Where(s => s != origin.NetworkModule.Socket);
            foreach (Socket s in socketToBroadcast)
                SocketUtils.Send(s, origin.NetworkModule.RemoteEndPoint + ": " + text);
        }
    }
}
