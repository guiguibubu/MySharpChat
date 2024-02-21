using MySharpChat.Core.Utils.Logger;

namespace MySharpChat.Server.Srv
{
    internal class SrvServerImpl : IServerImpl
    {
        private static readonly Logger logger = Logger.Factory.GetLogger<SrvServerImpl>();

        public ServerChatRoom ChatRoom { get; private set; }

        public SrvServerImpl()
        {
            ChatRoom = new ServerChatRoom(Guid.NewGuid());
        }

        public void Run(Server server)
        {
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }
    }
}
