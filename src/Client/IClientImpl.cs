using MySharpChat.Client.Command;
using MySharpChat.Client.Input;
using MySharpChat.Client.UI;
using MySharpChat.Core.Command;
using MySharpChat.Core.SocketModule;
using MySharpChat.Core.Utils;
using MySharpChat.Core.Utils.Logger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MySharpChat.Client
{
    internal interface IClientImpl : INetworkMachine
    {
        ClientOutputWriter OutputWriter { get; }

        bool Connect(IPEndPoint remoteEP, out bool isConnected, int timeoutMs = Timeout.Infinite);
        void Send(string? text);
        Task<string> ReadAsync(CancellationToken cancelToken = default);
        void Run(Client client);
    }
}
