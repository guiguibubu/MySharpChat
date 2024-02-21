﻿using MySharpChat.Core.Event;
using MySharpChat.Core.Model;
using MySharpChat.Core.Packet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MySharpChat.Client.GUI
{
    internal class GuiClientImpl : BaseClientImpl
    {
        public event Action<string> OnUserAddedEvent = (string username) => { };
        public event Action<string> OnUserRemovedEvent = (string username) => { };
        public event Action<string, string> OnUsernameChangeEvent = (string oldUsername, string newUsername) => { };
        public event Action OnLocalUsernameChangeEvent = () => { };
        public event Action<string> ChatMessageReceivedEvent = (string message) => { };
        public event Action<bool> DisconnectionEvent = (bool manual) => { };

        public bool ConnexionSuccess { get; set; } = false;

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
            if (ConnexionSuccess && networkModule.IsConnected())
            {
                if (networkModule.HasDataAvailable)
                {
                    HandleNetworkPackets(100);
                }
            }
        }

        private void HandleNetworkPacket(PacketWrapper? packet)
        {
            ArgumentNullException.ThrowIfNull(packet);

            if (packet.SourceId != ChatRoom.Id)
            {
                ChatRoom = new ChatRoom(packet.SourceId);
            }

            if (packet.Package is UserInfoPacket userInfoPacket)
            {
                HandleUserInfoPacket(userInfoPacket);

            }
            else if (packet.Package is ChatEvent chatEvent)
            {
                HandleChatEvent(chatEvent);
            }
        }

        private void HandleNetworkPackets(int nbMaxPacket = int.MaxValue)
        {
            int nbPacketsHandles = 0;
            while (networkModule.HasDataAvailable && nbPacketsHandles < nbMaxPacket)
            {
                PacketWrapper? packet = networkModule.CurrentData;
                HandleNetworkPacket(packet);
            }
        }

        private void HandleChatEvent(ChatEvent chatEvent)
        {
            if (!ChatEvents.Contains(chatEvent.Id))
            {
                ChatEvents.Add(chatEvent);
                if (chatEvent is ChatMessageEvent chatMessageEvent)
                {
                    HandleChatMessageEvent(chatMessageEvent);
                }
                if (chatEvent is ConnexionEvent connexionEvent)
                {
                    HandleConnexionEvent(connexionEvent);
                }
                if (chatEvent is DisconnexionEvent disconnexionEvent)
                {
                    HandleDisconnexionEvent(disconnexionEvent);
                }
                if (chatEvent is UsernameChangeEvent userChangeEvent)
                {
                    HandleUsernameChangeEvent(userChangeEvent);
                }
            }
        }

        private void HandleUserInfoPacket(UserInfoPacket userInfoPacket)
        {
            ArgumentNullException.ThrowIfNull(userInfoPacket);

            UserState userState = userInfoPacket.UserState;
            User user = userState.User;
            Guid userId = user.Id;
            string username = user.Username;

            if (LocalUser.Id == userId)
            {
                LocalUser.Username = username;
                OnLocalUsernameChangeEvent();
            }

            bool knownUser = ChatRoom.Users.Contains(userId);
            bool isConnected = userState.IsConnected();
            bool isDisconnection = knownUser && !isConnected;
            if (isDisconnection)
            {
                ChatRoom.Users.Remove(userId);
                OnUserRemovedEvent(username);
                return;
            }

            bool alreadyDiconnected = !knownUser && !isConnected;
            if (alreadyDiconnected)
                return;

            bool newUser = !knownUser && isConnected;
            if (newUser)
            {
                ChatRoom.Users.Add(new UserState(user, ConnexionStatus.GainConnection));
                OnUserAddedEvent(username);
                return;
            }

            User userInCache = ChatRoom.Users[userId].User;
            string oldUsername = userInCache.Username;
            if (oldUsername != username)
            {
                userInCache.Username = username;
                OnUsernameChangeEvent(oldUsername, username);
            }
        }

        private void HandleChatMessageEvent(ChatMessageEvent chatEvent)
        {
            ArgumentNullException.ThrowIfNull(chatEvent);

            ChatMessage chatMessage = chatEvent.ChatMessage;
            if (!ChatRoom.Messages.Contains(chatMessage.Id))
            {
                ChatRoom.Messages.Add(chatMessage);
                string username = chatMessage.User.Username;
                string messageText = chatMessage.Message;
                string readText = $"({chatMessage.Date}) {username} : {messageText}";
                ChatMessageReceivedEvent(readText);
            }
        }

        private void HandleConnexionEvent(ConnexionEvent connexionEvent)
        {
            ArgumentNullException.ThrowIfNull(connexionEvent);

            User user = connexionEvent.User;
            if (!ChatRoom.Users.Contains(user.Id))
            {
                ChatRoom.Users.Add(new UserState(user, ConnexionStatus.GainConnection));
                OnUserAddedEvent(user.Username);
            }
        }

        private void HandleDisconnexionEvent(DisconnexionEvent disconnexionEvent)
        {
            ArgumentNullException.ThrowIfNull(disconnexionEvent);

            User user = disconnexionEvent.User;
            if (ChatRoom.Users.Contains(user.Id))
            {
                ChatRoom.Users.Remove(user.Id);
                OnUserRemovedEvent(user.Username);
            }
        }

        private void HandleUsernameChangeEvent(UsernameChangeEvent userChangeEvent)
        {
            ArgumentNullException.ThrowIfNull(userChangeEvent);

            Guid userId = userChangeEvent.UidUser;
            if (ChatRoom.Users.Contains(userId))
            {
                User userInCache = ChatRoom.Users[userChangeEvent.UidUser].User;
                userInCache.Username = userChangeEvent.NewUsername;
                OnUsernameChangeEvent(userChangeEvent.OldUsername, userChangeEvent.NewUsername);
            }
        }
    }
}
