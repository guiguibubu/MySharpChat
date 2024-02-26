using MySharpChat.Core.API;
using MySharpChat.Core.Command;
using MySharpChat.Core.Constantes;
using MySharpChat.Core.Http;
using MySharpChat.Core.Model;
using MySharpChat.Core.Packet;
using MySharpChat.Core.Utils;
using System;
using System.Net.Http;

namespace MySharpChat.Client.Command
{
    public class UsernameCommand : Singleton<UsernameCommand>, IClientCommand
    {
        public string Name => "Username";

        protected UsernameCommand() { }

        public bool Execute(IClientImpl? client, params string[] args)
        {
            if (client is null)
                throw new ArgumentNullException(nameof(client));

            string? newUsername = args.Length > 0 ? args[0] : null;
            if (!string.IsNullOrEmpty(newUsername))
            {
                Guid userID = client.LocalUser.Id;
                User initPacket = new User(userID, newUsername);
                ClientNetworkModule clientNetworkModule = (ClientNetworkModule)client.NetworkModule;

                IUsersApi usersApi = RestEase.RestClient.For<IUsersApi>(clientNetworkModule.ServerUri);
                HttpResponseMessage httpResponseMessage = usersApi.PutUserAsync(userID.ToString(), initPacket).GetAwaiter().GetResult();

                return httpResponseMessage.IsSuccessStatusCode;
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
