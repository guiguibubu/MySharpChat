using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MySharpChat.Core.Utils
{
    public interface IConnectMachine
    {
        bool IsConnected();

        bool Connect(ConnexionInfos connexionInfos);
        bool Connect(IPEndPoint remoteEP, out bool isConnected, int timeoutMs = Timeout.Infinite);
        void Disconnect();
    }
}
