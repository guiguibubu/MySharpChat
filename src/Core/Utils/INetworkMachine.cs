using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MySharpChat.Core.Utils
{
    public interface INetworkMachine : IConnectMachine
    {
        string LocalEndPoint { get; }
        string RemoteEndPoint { get; }

        void Send(string? text);
        Task<string> ReadAsync(CancellationToken cancelToken = default);
    }
}
