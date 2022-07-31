using MySharpChat.Core.Utils;

namespace MySharpChat.Server
{
    public interface IServerImpl
    {
        INetworkModule NetworkModule { get; }

        void Run(Server server);
        void Stop();
    }
}
