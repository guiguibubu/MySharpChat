using MySharpChat.Core.Event;
using MySharpChat.Core.Model;
using MySharpChat.Core.Packet;
using MySharpChat.Core.Utils.Collection;
using MySharpChat.Core.Utils.Logger;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MySharpChat.Server
{
    public class ServerChatRoom : ChatRoom
    {
        private static readonly Logger logger = Logger.Factory.GetLogger<ServerChatRoom>();

        private readonly ChatEventCollection ChatEvents = new();

        public ServerChatRoom(Guid id) : base(id)
        {
        }

        public IEnumerable<PacketWrapper> ConnectUser(string? username, Guid userIdGuid)
        {
            if (string.IsNullOrEmpty(username))
            {
                username = "AnonymousUser";
            }
            if (!IsUserNameAvailable(userIdGuid, username))
            {
                username = GenerateNewUsername(userIdGuid, username);
            }

            UserState newUserState;
            User newUser;
            if (Users.Contains(userIdGuid))
            {
                newUserState = Users[userIdGuid];
                newUser = newUserState.User;
                newUser.Username = username;
                newUserState.AddConnexionEvent(ConnexionStatus.GainConnection);
            }
            else
            {
                newUser = new User(userIdGuid, username);
                newUserState = new UserState(newUser, ConnexionStatus.GainConnection);
                Users.Add(newUserState);
            }
            logger.LogInfo("New user connected : {0}", newUser);
            ChatEvents.Add(new ConnexionEvent(newUser));

            List<PacketWrapper> responsePackets = new();
            PacketWrapper initPacket = new PacketWrapper(Id, new UserInfoPacket(newUserState));
            responsePackets.Add(initPacket);

            foreach (ChatMessage chatMessage in Messages)
            {
                PacketWrapper chatPacket = new PacketWrapper(Id, new ChatMessagePacket(chatMessage));
                responsePackets.Add(chatPacket);
            }

            return responsePackets;
        }

        public void DisconnectUser(Guid userIdGuid)
        {
            User user = Users[userIdGuid].User;
            logger.LogInfo("Disconnection of : {0}", user);
            Users[userIdGuid].AddConnexionEvent(ConnexionStatus.LostConnection);
            ChatEvents.Add(new DisconnexionEvent(user));
        }

        public void AddMessage(Guid userIdGuid, ChatMessage message)
        {
            User user = Users[userIdGuid].User;
            logger.LogInfo("Message received from {0} => {1}", user, message.Message);
            Messages.Add(message);
            ChatEvents.Add(new ChatMessageEvent(message));
        }

        public IEnumerable<PacketWrapper> GetChatEvents(string? lastId)
        {
            List<PacketWrapper> packets = new();
            IReadOnlyCollection<ChatEvent> eventToSend;
            if (string.IsNullOrEmpty(lastId))
            {
                eventToSend = ChatEvents;
            }
            else
            {
                ChatEvent lastEventReceived = ChatEvents[Guid.Parse(lastId)];
                List<ChatEvent> eventOrdered = ChatEvents.OrderByDescending(chatEvent => chatEvent.Date).ToList();
                int indexLastEvent = eventOrdered.IndexOf(lastEventReceived);
                eventToSend = eventOrdered.GetRange(0, indexLastEvent);
            }
            foreach (ChatEvent chatEvent in eventToSend)
            {
                PacketWrapper packet = new ChatEventPacketWrapper(Id, chatEvent);
                packets.Add(packet);
            }
            return packets;
        }

        public IEnumerable<PacketWrapper> GetUsers()
        {
            List<PacketWrapper> packets = new();
            foreach (UserState userState in Users)
            {
                PacketWrapper packet = new PacketWrapper(Id, new UserInfoPacket(userState));
                packets.Add(packet);
            }
            return packets;
        }

        public bool ModifyUser(Guid userIdGuid, string newUsername)
        {
            if (string.IsNullOrEmpty(newUsername) || IsUserNameAvailable(userIdGuid, newUsername))
            {
                if (!string.IsNullOrEmpty(newUsername))
                {
                    User user = Users[userIdGuid].User;
                    string oldUsername = user.Username;
                    logger.LogInfo("Username change from {1} to {2} for {0}", user, oldUsername, newUsername);
                    user.Username = newUsername;
                    ChatEvents.Add(new UsernameChangeEvent(userIdGuid, oldUsername, newUsername));
                }
                return true;
            }
            else
            {
                string errorMessage = $"This username is already used : \"{newUsername}\"";
                logger.LogError(errorMessage);

                return false;
            }
        }

        private bool IsUserNameAvailable(Guid userId, string username)
        {
            return !Users.Where(user => user.Id != userId).Where(userState => IsUserConnected(userState.Id)).Select(userState => userState.User.Username).Contains(username);
        }

        private string GenerateNewUsername(Guid userId, string currentUsername)
        {
            string newUsername = currentUsername;
            int usernameSuffix = 1;
            while (!IsUserNameAvailable(userId, newUsername))
            {
                newUsername = currentUsername + "_" + usernameSuffix;
                usernameSuffix++;
            }
            return newUsername;
        }

        public bool IsUserConnected(Guid userId)
        {
            bool userExist = Users.Contains(userId);
            if (!userExist)
                return false;

            UserState userState = Users[userId];
            return userState.IsConnected();
        }
    }
}
