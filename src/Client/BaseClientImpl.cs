using MySharpChat.Core.Utils;
using System;

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
