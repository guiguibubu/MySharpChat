using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MySharpChat.Core.Utils
{
    public interface IConnectMachine
    {
        bool IsConnected();

        bool Connect(ConnexionInfos connexionInfos);
        void Disconnect();
    }
}
