using MySharpChat.Client.UI;

namespace MySharpChat.Client
{
    internal interface IClientImpl
    {
        ClientOutputWriter OutputWriter { get; }
        ClientNetworkModule NetworkModule { get; }
        
        IClientLogic CurrentLogic { get; set; }

        void Run(Client client);
        void Stop();
    }
}
