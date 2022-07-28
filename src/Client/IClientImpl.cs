using MySharpChat.Core.UI;
using MySharpChat.Core.Utils;

namespace MySharpChat.Client
{
    public interface IClientImpl
    {
        IUserInterfaceModule UserInterfaceModule { get; }
        INetworkModule NetworkModule { get; }
        
        IClientLogic CurrentLogic { get; set; }

        void Run(Client client);
        void Stop();
    }
}
