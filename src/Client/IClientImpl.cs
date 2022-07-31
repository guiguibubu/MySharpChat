using MySharpChat.Core.UI;
using MySharpChat.Core.Utils;
using System;

namespace MySharpChat.Client
{
    public interface IClientImpl
    {
        IUserInterfaceModule UserInterfaceModule { get; }
        INetworkModule NetworkModule { get; }

        IClientLogic CurrentLogic { get; set; }

        public Guid ClientId { get; }

        void Run(Client client);
        void Stop();
    }
}
