using MySharpChat.Core.Utils;
using System;

namespace MySharpChat.Server
{
    public interface IServerImpl
    {
        INetworkModule NetworkModule { get; }

        ServerChatRoom ChatRoom { get; }

        void Run(Server server);
        void Start();
        void Stop();
    }
}
