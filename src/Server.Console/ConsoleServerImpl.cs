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

        public Guid ServerId { get; private set; } = Guid.NewGuid();

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
                    session.OnSessionInitializedCallback += OnSessionInitialized;
                    session.OnSessionFinishedCallback += OnSessionFinished;
                    session.OnBroadcastCallback += Broadcast;
                    session.OnUsernameChangeCallback += OnUsernameChange;
                    session.Start(ServerId);
                }
            );
        }

        private void OnSessionInitialized(ChatSession session)
        {
            bool usernameAlreadyUsed = m_connectedSessions.Select(s => s.ClientUsername).Contains(session.ClientUsername);
            int usernameSuffix = 1;
            string oldUsername = session.ClientUsername;
            while (usernameAlreadyUsed)
            {
                session.ClientUsername = oldUsername + "_" + usernameSuffix;
                usernameAlreadyUsed = m_connectedSessions.Select(s => s.ClientUsername).Contains(session.ClientUsername);
            }

            ClientInitialisationPacket connectInitPacket = new ClientInitialisationPacket(session.ClientId, session.ClientUsername);
            session.NetworkModule.Send(new PacketWrapper(ServerId, connectInitPacket));

            string message = $"New user joined : {session.ClientUsername}";
            ChatPacket package = new ChatPacket(message);
            PacketWrapper packet = new PacketWrapper(ServerId, package);

            foreach (ChatSession s in m_connectedSessions)
            {
                s.NetworkModule.Send(packet);
            }

            m_connectedSessions.Add(session);

        }

        private void OnSessionFinished(ChatSession session)
        {
            m_connectedSessions.Remove(session);

            string message = $"User leave the session : {session.ClientUsername}";
            ChatPacket package = new ChatPacket(message);
            PacketWrapper packet = new PacketWrapper(ServerId, package);

            foreach (ChatSession s in m_connectedSessions)
            {
                s.NetworkModule.Send(packet);
            }
        }

        private void Broadcast(ChatSession origin, string text)
        {
            IEnumerable<ChatSession> sessionToBroadcast = m_connectedSessions.Where(s => s != origin);

            string message = origin.ClientUsername + ": " + text;
            ChatPacket package = new ChatPacket(message);
            PacketWrapper packet = new PacketWrapper(origin.ClientId, package);

            foreach (ChatSession session in sessionToBroadcast)
            {
                session.NetworkModule.Send(packet);
            }
        }

        private void OnUsernameChange(ChatSession session, string oldUsername)
        {
            bool usernameChanged = string.Equals(session.ClientUsername, oldUsername, StringComparison.InvariantCulture);
            if(!usernameChanged)
                return;

            string currentUsername = session.ClientUsername;

            bool usernameAlreadyUsed = m_connectedSessions.Select(s => s.ClientUsername).Contains(currentUsername);
            int usernameSuffix = 1;
            while (usernameAlreadyUsed)
            {
                session.ClientUsername = currentUsername + "_" + usernameSuffix;
                usernameAlreadyUsed = m_connectedSessions.Select(s => s.ClientUsername).Contains(session.ClientUsername);
            }

            ClientInitialisationPacket connectInitPacket = new ClientInitialisationPacket(session.ClientId, session.ClientUsername);
            session.NetworkModule.Send(new PacketWrapper(ServerId, connectInitPacket));

            string message = $"User name changed for \"{oldUsername}\" to \"{session.ClientUsername}\"";
            ChatPacket package = new ChatPacket(message);
            PacketWrapper packet = new PacketWrapper(ServerId, package);

            IEnumerable<ChatSession> sessionToBroadcast = m_connectedSessions.Where(s => s != session);
            foreach (ChatSession s in sessionToBroadcast)
            {
                s.NetworkModule.Send(packet);
            }
        }
    }
}
