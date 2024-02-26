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

        public void ConnectUser(string? username, Guid userId)
        {
            if (string.IsNullOrEmpty(username))
            {
                username = "AnonymousUser";
            }
            if (!IsUserNameAvailable(userId, username))
            {
                username = GenerateNewUsername(userId, username);
            }

            UserState newUserState;
            User newUser;
            if (Users.Contains(userId))
            {
                newUserState = Users[userId];
                newUser = newUserState.User;
                newUser.Username = username;
                newUserState.AddConnexionEvent(ConnexionStatus.GainConnection);
            }
            else
            {
                newUser = new User(userId, username);
                newUserState = new UserState(newUser, ConnexionStatus.GainConnection);
                Users.Add(newUserState);
            }
            logger.LogInfo("New user connected : {0}", newUser);
            ChatEvents.Add(new ConnexionEvent(newUser));
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

        public ChatMessage? GetMessage(Guid messageId)
        {
            if (Messages.TryGet(messageId, out ChatMessage? chatMessage))
            {
                return chatMessage;
            }
            else
            {
                return null;
            }
        }

        public IReadOnlyCollection<ChatEventPacketWrapper> GetChatEventPackets(string? lastId)
        {
            IReadOnlyCollection<ChatEvent> eventToSend = GetChatEvents(lastId);
            return eventToSend.Select(chatEvent => new ChatEventPacketWrapper(Id, chatEvent)).ToList();
        }

        public IReadOnlyCollection<ChatEvent> GetChatEvents(string? lastId)
        {
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
                eventToSend = eventToSend.Reverse().ToList();
            }
            return eventToSend;
        }

        public IReadOnlyCollection<PacketWrapper<User>> GetUserPackets()
        {
            IReadOnlyCollection<User> userinfos = GetUsers();
            return userinfos.Select(userInfo => new PacketWrapper<User>(Id, userInfo)).ToList();
        }

        public IReadOnlyCollection<User> GetUsers()
        {
            return Users.Where(u => u.IsConnected()).Select(userState => userState.User).ToList();
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
