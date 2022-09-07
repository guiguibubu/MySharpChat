using MySharpChat.Core.Model;
using MySharpChat.Core.Utils;
using System;

namespace MySharpChat.Client
{
    public interface IClientImpl
    {
        INetworkModule NetworkModule { get; }

        User LocalUser { get; }
        ChatRoom? ChatRoom { get; set; }

        void Run(Client client);
        void Stop();
    }
}
