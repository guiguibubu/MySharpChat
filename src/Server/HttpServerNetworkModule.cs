using MySharpChat.Core.Packet;
using MySharpChat.Core.NetworkModule;
using MySharpChat.Core.Utils.Logger;
using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using MySharpChat.Core.Http;
using MySharpChat.Server.Utils;

namespace MySharpChat.Server
{
    public class HttpServerNetworkModule : IServerNetworkModule
    {
        private static readonly Logger logger = Logger.Factory.GetLogger<HttpServerNetworkModule>();

        private readonly HttpServer httpServer;

        public HttpServerNetworkModule()
        {
            httpServer = new HttpServer();
        }

        public bool HasDataAvailable => !httpServer.requestQueue.IsEmpty;

        public HttpListenerContext? CurrentData
        {
            get
            {
                HttpListenerContext? context;
                while (!httpServer.requestQueue.TryDequeue(out context)) { }


                return context;
            }
        }

        public bool Connect(IPEndPoint remoteEP, int timeoutMs = Timeout.Infinite)
        {
            throw new NotImplementedException("Use Connect(ConnexionInfos connexionInfos) instead");
        }

        public bool Connect(ConnexionInfos connexionInfos)
        {
            ConnexionInfos.Data? connexionData = connexionInfos.Local;
            if (connexionData == null)
                throw new ArgumentException(nameof(connexionInfos.Local));

            IPEndPoint localEP = NetworkUtils.CreateEndPoint(connexionData);

            // Create a HTTP Server.
            httpServer.Start(localEP);

            logger.LogInfo(string.Format("Listenning at {0}", httpServer.Prefixes.First()));

            return true;
        }

        public Task<bool> ConnectAsync(ConnexionInfos connexionInfos)
        {
            return Task.Run(() => Connect(connexionInfos));
        }

        public Task<bool> ConnectAsync(IPEndPoint remoteEP, int timeoutMs = Timeout.Infinite)
        {
            throw new NotImplementedException("Use ConnectAsync(ConnexionInfos connexionInfos) instead");
        }

        public void Disconnect()
        {
            if (httpServer != null)
            {
                httpServer.Stop();
            }
        }

        public bool IsConnected()
        {
            return httpServer != null && httpServer.IsRunning;
        }

        public Task<HttpResponseMessage?> Send(HttpSendRequestContext context, PacketWrapper? packet)
        {
            throw new NotImplementedException("Server should not be able to send data");
        }

        public HttpResponseMessage? Read(HttpReadRequestContext context, TimeSpan timeoutSpan)
        {
            throw new NotImplementedException("Server should not be able to read data");
        }
    }
}
