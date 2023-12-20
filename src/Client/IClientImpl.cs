using MySharpChat.Client.Utils;
using MySharpChat.Core.Model;
using MySharpChat.Core.Utils.Collection;
using System.Collections.Generic;

namespace MySharpChat.Client
{
    public interface IClientImpl
    {
        IClientNetworkModule NetworkModule { get; }

        User LocalUser { get; }
        ChatEventCollection ChatEvents { get; }

        void Initialize(object? initObject = null);
        void Run(Client client);
        void Stop();
    }
}
