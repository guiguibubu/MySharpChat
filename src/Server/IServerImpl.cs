namespace MySharpChat.Server
{
    public interface IServerImpl
    {
        ServerChatRoom ChatRoom { get; }

        void Run(Server server);
        void Start();
        void Stop();
    }
}
