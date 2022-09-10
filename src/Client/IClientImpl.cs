using MySharpChat.Client.Utils;
using MySharpChat.Core.Model;

namespace MySharpChat.Client
{
    public interface IClientImpl
    {
        IClientNetworkModule NetworkModule { get; }

        User LocalUser { get; }
        ChatRoom? ChatRoom { get; set; }

        void Run(Client client);
        void Stop();
    }
}
