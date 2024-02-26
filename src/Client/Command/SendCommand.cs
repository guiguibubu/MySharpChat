using System;
using System.Net.Http;
using MySharpChat.Core;
using MySharpChat.Core.API;
using MySharpChat.Core.Constantes;
using MySharpChat.Core.Http;
using MySharpChat.Core.Model;
using MySharpChat.Core.Packet;
using MySharpChat.Core.Utils;

namespace MySharpChat.Client.Command
{
    public class SendCommand : Singleton<SendCommand>, IClientCommand
    {
        protected SendCommand() { }

        public string Name => "Send";

        public bool Execute(IClientImpl? client, params string[] args)
        {
            if (client is null)
                throw new ArgumentNullException(nameof(client));

            if (client.LocalUser.Id == Guid.Empty)
                throw new ConnectionNotInitializedException();

            string? text = args.Length > 0 ? args[0] : null;
            if (!string.IsNullOrEmpty(text))
            {
                ChatMessage chatMessage = new ChatMessage(Guid.NewGuid(), client.LocalUser, DateTime.Now, text);
                ClientNetworkModule clientNetworkModule = (ClientNetworkModule)client.NetworkModule;
                IMessagesApi messagesApi = RestEase.RestClient.For<IMessagesApi>(clientNetworkModule.ServerUri);
                HttpResponseMessage httpResponseMessage = messagesApi.PostMessageAsync(client.LocalUser.Id.ToString(), chatMessage).GetAwaiter().GetResult();

                return httpResponseMessage.IsSuccessStatusCode;
            }
            else
            {
                return false;
            }
        }

        public string GetHelp()
        {
            return "usage: send <text>";
        }

        public string GetSummary()
        {
            return "Command to send messages";
        }
    }
}
