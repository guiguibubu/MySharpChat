using MySharpChat.Core.Http;
using MySharpChat.Core.Packet;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace MySharpChat.Core.Utils
{
    public interface INetworkModule<T> : IConnectMachine
    {
        bool HasDataAvailable { get; }
        T? CurrentData { get; }
        Task<HttpResponseMessage?> Send(HttpSendRequestContext context, PacketWrapper? packet);
        HttpResponseMessage? Read(HttpReadRequestContext context, TimeSpan timeoutSpan);
        public HttpResponseMessage? Read(HttpReadRequestContext context)
        {
            return Read(context, Timeout.InfiniteTimeSpan);
        }
    }
}
