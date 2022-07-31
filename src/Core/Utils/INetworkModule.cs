using MySharpChat.Core.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MySharpChat.Core.Utils
{
    public interface INetworkModule : IConnectMachine
    {
        string LocalEndPoint { get; }
        string RemoteEndPoint { get; }
        bool HasDataAvailable { get; }

        void Send(PacketWrapper? packet);

        PacketWrapper Read(TimeSpan timeoutSpan);
        PacketWrapper Read()
        {
            return Read(Timeout.InfiniteTimeSpan);
        }
    }
}
