using MySharpChat.Core.Model;
using MySharpChat.Core.Packet;
using System;
using System.Collections.Generic;

namespace MySharpChat.Client.GUI.MAUI
{
    internal class GuiClientImpl : BaseClientImpl
    {
        public event Action<string> OnUserAddedEvent = (string username) => { };
        public event Action<string> OnUserRemovedEvent = (string username) => { };
        public event Action<string, string> OnUsernameChangeEvent = (string oldUsername, string newUsername) => { };
        public event Action OnLocalUsernameChangeEvent = () => { };
        public event Action<string> ChatMessageReceivedEvent = (string message) => { };
        public event Action<bool> DisconnectionEvent = (bool manual) => { };

        public GuiClientImpl() : base()
        { }


        public override void Initialize(object? initObject = null)
        {
        }

        public void SetUsername(string? username)
        {
            if (!string.IsNullOrEmpty(username))
                LocalUser.Username = username;
        }

        public override void Run(Client client)
        {
            if (networkModule.IsConnected())
            {
                if (networkModule.HasDataAvailable)
                {
                    HandleNetworkPackets(100);
                }
            }
        }

        private void HandleNetworkPacket(PacketWrapper packet)
        {
            if (packet.Package is UserInfoPacket userInfoPacket)
            {
                UserState userState = userInfoPacket.UserState;
                User user = userState.User;
                Guid userId = user.Id;
                string username = user.Username;

                if (LocalUser.Id == userId)
                {
                    LocalUser.Username = username;
                    OnLocalUsernameChangeEvent();
                }

                bool knownUser = ChatRoom!.Users.Contains(userId);
                bool isConnected = userState.IsConnected();
                bool isDisconnection = knownUser && !isConnected;
                if (isDisconnection)
                {
                    ChatRoom!.Users.Remove(userId);
                    OnUserRemovedEvent(username);
                    return;
                }

                bool alreadyDiconnected = !knownUser && !isConnected;
                if (alreadyDiconnected)
                    return;

                bool newUser = !knownUser && isConnected;
                if (newUser)
                {
                    ChatRoom!.Users.Add(new UserState(user, ConnexionStatus.GainConnection));
                    OnUserAddedEvent(username);
                    return;
                }

                User userInCache = ChatRoom!.Users[userId].User;
                string oldUsername = userInCache.Username;
                if (oldUsername != username)
                {
                    userInCache.Username = username;
                    OnUsernameChangeEvent(oldUsername, username);
                }
            }
            else if (packet.Package is ChatMessagePacket chatPackage)
            {
                HandleChatPacket(chatPackage);
            }
        }

        private void HandleNetworkPackets(int nbMaxPacket = int.MaxValue)
        {
            int nbPacketsHandles = 0;
            while (networkModule.HasDataAvailable && nbPacketsHandles < nbMaxPacket)
            {
                PacketWrapper packet = networkModule.CurrentData;
                HandleNetworkPacket(packet);
            }
        }

        private void HandleChatPacket(ChatMessagePacket chatPacket)
        {
            ChatMessage chatMessage = chatPacket.ChatMessage;
            if (!ChatRoom!.Messages.Contains(chatMessage))
            {
                ChatRoom!.Messages.Add(chatMessage);
                string username = chatMessage.User.Username;
                string messageText = chatMessage.Message;
                string readText = $"({chatMessage.Date}) {username} : {messageText}";
                ChatMessageReceivedEvent(readText);
            }
        }
    }
}
