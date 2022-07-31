using MySharpChat.Core.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MySharpChat.Server
{
    public class ChatRoom
    {
        private readonly List<ChatSession> m_connectedSessions = new List<ChatSession>();
        private readonly Guid _serverId;

        public ChatRoom(Guid serverId)
        {
            _serverId = serverId;
        }

        public void LaunchSession(Socket? socket)
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
                session.Start(_serverId);
            }
            );
        }


        private void OnSessionInitialized(ChatSession session)
        {
            UpdateUsernameIfNecessary(session);

            string message = $"New user joined : {session.ClientUsername}";
            ChatPacket package = new ChatPacket(message);
            PacketWrapper packet = new PacketWrapper(_serverId, package);

            Broadcast(session, packet);

            m_connectedSessions.Add(session);

        }

        private void OnSessionFinished(ChatSession session)
        {
            m_connectedSessions.Remove(session);

            string message = $"User leave the session : {session.ClientUsername}";
            ChatPacket package = new ChatPacket(message);
            PacketWrapper packet = new PacketWrapper(_serverId, package);

            Broadcast(session, packet);
        }

        private void OnUsernameChange(ChatSession session, string oldUsername)
        {
            bool usernameChanged = string.Equals(session.ClientUsername, oldUsername, StringComparison.InvariantCulture);
            if (!usernameChanged)
                return;

            UpdateUsernameIfNecessary(session);

            string message = $"User name changed for \"{oldUsername}\" to \"{session.ClientUsername}\"";
            ChatPacket package = new ChatPacket(message);
            PacketWrapper packet = new PacketWrapper(_serverId, package);

            Broadcast(session, packet);
        }

        private void Broadcast(ChatSession origin, PacketWrapper packet)
        {
            IEnumerable<ChatSession> sessionToBroadcast = m_connectedSessions.Where(s => s != origin);
            foreach (ChatSession session in sessionToBroadcast)
            {
                session.NetworkModule.Send(packet);
            }
        }

        private bool IsUserNameAvailable(ChatSession session, string username)
        {
            return !m_connectedSessions.Where(s => s != session).Select(s => s.ClientUsername).Contains(username);
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
            string currentUsername = session.ClientUsername;

            if (!IsUserNameAvailable(session, currentUsername))
            {
                session.ClientUsername = GenerateNewUsername(session, currentUsername);
            }

            ClientInitialisationPacket connectInitPacket = new ClientInitialisationPacket(session.ClientId, session.ClientUsername);
            session.NetworkModule.Send(new PacketWrapper(_serverId, connectInitPacket));
        }
    }
}
