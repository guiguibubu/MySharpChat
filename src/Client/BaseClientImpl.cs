using System;
using MySharpChat.Client.Utils;
using MySharpChat.Core.Model;
using MySharpChat.Core.Utils.Collection;

namespace MySharpChat.Client
{
    public abstract class BaseClientImpl : IClientImpl
    {
        protected readonly IClientNetworkModule networkModule;
        public IClientNetworkModule NetworkModule => networkModule;

        public User LocalUser { get; protected set; } = new User(Guid.NewGuid(), Environment.UserName);
        public ChatRoom? ChatRoom { get; set; } = null;
        public ChatEventCollection ChatEvents { get; } = new();

        protected BaseClientImpl()
        {
            networkModule = new ClientNetworkModule(this);
        }

        public abstract void Initialize(object? initObject = null);
        public abstract void Run(Client client);

        public virtual void Stop()
        {
            networkModule.Disconnect();
        }
    }
}
