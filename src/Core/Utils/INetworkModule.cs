using MySharpChat.Core.Http;
using MySharpChat.Core.Packet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MySharpChat.Core.Utils
{
    public interface INetworkModule : IConnectMachine
    {
        bool HasDataAvailable { get; }

        Task<HttpResponseMessage?> Send(HttpSendRequestContext context, PacketWrapper? packet);
        HttpResponseMessage? Read(HttpReadRequestContext context, TimeSpan timeoutSpan);
        public HttpResponseMessage? Read(HttpReadRequestContext context)
        {
            return Read(context, Timeout.InfiniteTimeSpan);
        }
    }
}
