using System;
using MySharpChat.Core;
using MySharpChat.Core.Http;
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
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            if (client.LocalUser.Id == Guid.Empty)
                throw new ConnectionNotInitializedException();

            string? text = args.Length > 0 ? args[0] : null;
            if (!string.IsNullOrEmpty(text))
            {
                ChatMessagePacket chatPacket = new ChatMessagePacket(Guid.NewGuid(), client.LocalUser, DateTime.Now, text);
                PacketWrapper packet = new PacketWrapper(client.LocalUser.Id, chatPacket);
                ClientNetworkModule clientNetworkModule = (ClientNetworkModule)client.NetworkModule;
                UriBuilder requestUriBuilder = new UriBuilder(clientNetworkModule.ChatUri!);
                requestUriBuilder.Path += "/message";
                requestUriBuilder.Query = $"userId={client.LocalUser.Id}";
                clientNetworkModule.Send(HttpSendRequestContext.Post(requestUriBuilder.Uri), packet);

                return true;
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
