using MySharpChat.Server.Utils;

namespace MySharpChat.Server
{
    public interface IServerImpl
    {
        IServerNetworkModule NetworkModule { get; }

        ServerChatRoom ChatRoom { get; }

        void Run(Server server);
        void Start();
        void Stop();
    }
}
