using MySharpChat.Client.Command;
using MySharpChat.Client.Input;
using MySharpChat.Core.Command;
using MySharpChat.Core.Packet;
using MySharpChat.Core.UI;
using MySharpChat.Core.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySharpChat.Client
{
    public abstract class BaseClientImpl : IClientImpl
    {
        protected readonly INetworkModule networkModule;
        public INetworkModule NetworkModule => networkModule;

        public string LocalEndPoint => networkModule.LocalEndPoint;

        public string RemoteEndPoint => networkModule.RemoteEndPoint;

        public Guid ClientId { get; protected set; } = Guid.Empty;
        public string Username { get; protected set; } = Environment.UserName;

        protected BaseClientImpl()
        {
            networkModule = new ClientNetworkModule(this);
        }

        public abstract void Run(Client client);

        public virtual void Stop()
        {
            networkModule.Disconnect();
        }
    }
}
