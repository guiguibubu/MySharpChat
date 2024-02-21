using MySharpChat.Core.Http;
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
        Task<HttpResponseMessage?> SendAsync<U>(HttpSendRequestContext context, U? packet);
        Task<HttpResponseMessage?> SendAsync(HttpSendRequestContext context);
        HttpResponseMessage? Read(HttpReadRequestContext context, TimeSpan timeoutSpan);
        public HttpResponseMessage? Read(HttpReadRequestContext context)
        {
            return Read(context, Timeout.InfiniteTimeSpan);
        }
    }
}
