using MySharpChat.Core.UI;
using MySharpChat.Core.Utils;
using System;

namespace MySharpChat.Client
{
    public interface IClientImpl
    {
        INetworkModule NetworkModule { get; }

        Guid ClientId { get; }
        string Username { get; }

        void Run(Client client);
        void Stop();
    }
}
