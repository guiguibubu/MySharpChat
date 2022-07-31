using MySharpChat.Core.Utils;
using System;

namespace MySharpChat.Server
{
    public interface IServerImpl
    {
        INetworkModule NetworkModule { get; }

        Guid ServerId { get; }

        void Run(Server server);
        void Stop();
    }
}
