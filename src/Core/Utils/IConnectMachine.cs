using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace MySharpChat.Core.Utils
{
    public interface IConnectMachine
    {
        bool IsConnected();

        bool Connect(ConnexionInfos connexionInfos);
        bool Connect(IPEndPoint remoteEP, int timeoutMs = Timeout.Infinite);
        Task<bool> ConnectAsync(ConnexionInfos connexionInfos);
        Task<bool> ConnectAsync(IPEndPoint remoteEP, int timeoutMs = Timeout.Infinite);
        void Disconnect();
    }
}
