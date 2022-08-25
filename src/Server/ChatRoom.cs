using MySharpChat.Core.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace MySharpChat.Server
{
    public class ChatRoom
    {
        private readonly List<ChatSession> m_connectedSessions = new List<ChatSession>();
        private readonly List<ChatMessage> m_messages = new List<ChatMessage>();
        private readonly Guid _serverId;

        public ChatRoom(Guid serverId)
        {
            _serverId = serverId;
        }

        public void LaunchSession(TcpClient tcpClient)
        {
            if (tcpClient == null)
                throw new ArgumentNullException(nameof(tcpClient));

            Task.Run(() =>
            {
                ChatSession session = new ChatSession(tcpClient);
                session.OnSessionInitializedCallback += OnSessionInitialized;
                session.OnSessionFinishedCallback += OnSessionFinished;
                session.OnBroadcastCallback += Broadcast;
                session.OnUsernameChangeCallback += OnUsernameChange;
                session.Start(_serverId);
            }
            );
        }


        private void OnSessionInitialized(ChatSession session)
        {
            UpdateUsernameIfNecessary(session);

            UserStatusPacket package = new UserStatusPacket(session.User.Username, true);
            PacketWrapper packet = new PacketWrapper(_serverId, package);

            Broadcast(session, packet);

            foreach (ChatMessage message in m_messages)
            {
                ChatPacket chatPackage = new ChatPacket(message.Message);
                PacketWrapper messagePacket = new PacketWrapper(_serverId, chatPackage);
                session.NetworkModule.Send(messagePacket);
            }

            foreach (ChatSession connectedSession in m_connectedSessions)
            {
                UserStatusPacket userPackage = new UserStatusPacket(connectedSession.User.Username, true);
                PacketWrapper messagePacket = new PacketWrapper(_serverId, userPackage);
                session.NetworkModule.Send(messagePacket);
            }

            m_connectedSessions.Add(session);

        }

        private void OnSessionFinished(ChatSession session)
        {
            m_connectedSessions.Remove(session);

            UserStatusPacket package = new UserStatusPacket(session.User.Username, false);
            PacketWrapper packet = new PacketWrapper(_serverId, package);

            Broadcast(session, packet);
        }

        private void OnUsernameChange(ChatSession session, string oldUsername)
        {
            bool usernameChanged = !string.Equals(session.User.Username, oldUsername, StringComparison.InvariantCulture);
            if (!usernameChanged)
                return;

            UpdateUsernameIfNecessary(session);

            string message = $"User name changed for \"{oldUsername}\" to \"{session.User.Username}\"";
            ChatPacket package = new ChatPacket(message);
            PacketWrapper packet = new PacketWrapper(_serverId, package);

            Broadcast(session, packet);
        }

        private void Broadcast(ChatSession origin, PacketWrapper packet)
        {
            if (packet.Package is ChatPacket chatPacket)
                m_messages.Add(new ChatMessage(origin.User, chatPacket.Message));

            IEnumerable<ChatSession> sessionToBroadcast = m_connectedSessions;
            foreach (ChatSession session in sessionToBroadcast)
            {
                session.NetworkModule.Send(packet);
            }
        }

        private bool IsUserNameAvailable(ChatSession session, string username)
        {
            return !m_connectedSessions.Where(s => s != session).Select(s => s.User.Username).Contains(username);
        }

        private string GenerateNewUsername(ChatSession session, string currentUsername)
        {
            string newUsername = currentUsername;
            int usernameSuffix = 1;
            while (!IsUserNameAvailable(session, newUsername))
            {
                newUsername = currentUsername + "_" + usernameSuffix;
                usernameSuffix++;
            }
            return newUsername;
        }

        private void UpdateUsernameIfNecessary(ChatSession session)
        {
            string currentUsername = session.User.Username;

            if (!IsUserNameAvailable(session, currentUsername))
            {
                session.User.Username = GenerateNewUsername(session, currentUsername);
            }

            ClientInitialisationPacket connectInitPacket = new ClientInitialisationPacket(session.ClientId, session.User.Username);
            session.NetworkModule.Send(new PacketWrapper(_serverId, connectInitPacket));
        }
    }
}
