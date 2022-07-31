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

        private readonly List<ChatSession> m_connectedSessions = new List<ChatSession>();

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
                    m_connectedSessions.Add(session);
                    session.OnSessionFinishedCallback += OnSessionFinished;
                    session.OnBroadcastCallback += Broadcast;
                    session.Start();
                }
            );
        }

        private void OnSessionFinished(ChatSession session)
        {
            m_connectedSessions.Remove(session);
        }

        private void Broadcast(ChatSession origin, string text)
        {
            IEnumerable<ChatSession> sessionToBroadcast = m_connectedSessions.Where(s => s != origin);
            foreach (ChatSession session in sessionToBroadcast)
            {
                string message = origin.NetworkModule.RemoteEndPoint + ": " + text;
                ChatPacket package = new ChatPacket(message);
                string packetId = origin.NetworkModule.LocalEndPoint;
                PacketWrapper packet = new PacketWrapper(packetId, package);
                session.NetworkModule.Send(packet);
            }
        }
    }
}
