﻿using System;
using MySharpChat.Core.Http;
using MySharpChat.Core.Model;
using MySharpChat.Core.Packet;
using MySharpChat.Core.Utils;

namespace MySharpChat.Client.Command
{
    public class UsernameCommand : Singleton<UsernameCommand>, IClientCommand
    {
        public string Name => "Username";

        protected UsernameCommand() { }

        public bool Execute(IClientImpl? client, params string[] args)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            string? newUsername = args.Length > 0 ? args[0] : null;
            if (!string.IsNullOrEmpty(newUsername))
            {
                Guid userID = client.LocalUser.Id;
                UserInfoPacket initPacket = new UserInfoPacket(userID, newUsername, ConnexionStatus.GainConnection);
                PacketWrapper packet = new PacketWrapper(userID, initPacket);
                ClientNetworkModule clientNetworkModule = (ClientNetworkModule)client.NetworkModule;
                UriBuilder requestUriBuilder = new UriBuilder(clientNetworkModule.ChatUri!);
                requestUriBuilder.Path += "/user";
                requestUriBuilder.Query = $"userId={userID}";
                clientNetworkModule.Send(HttpSendRequestContext.Put(requestUriBuilder.Uri), packet);

                return true;
            }
            else
            {
                return false;
            }
        }

        public string GetHelp()
        {
            return "usage: username <new_username>";
        }

        public string GetSummary()
        {
            return "Command to change your username.";
        }
    }
}
