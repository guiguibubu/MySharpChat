using MySharpChat.Core.Packet;
using MySharpChat.Core.NetworkModule;
using MySharpChat.Core.Utils;
using MySharpChat.Core.Utils.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Net.Http;
using MySharpChat.Core.Http;

namespace MySharpChat.Server
{
    internal class ConsoleServerImpl : IServerImpl
    {
        private static readonly Logger logger = Logger.Factory.GetLogger<ConsoleServerImpl>();

        private readonly ServerNetworkModule networkModule;
        public INetworkModule NetworkModule => networkModule;

        private Dictionary<string, IHttpRequestHandler> httpHandlersCache=  new Dictionary<string, IHttpRequestHandler>();

        public ServerChatRoom ChatRoom { get; private set; }

        public ConsoleServerImpl()
        {
            networkModule = new ServerNetworkModule();
            ChatRoom = new ServerChatRoom(Guid.NewGuid());
            httpHandlersCache.Add("chat", ChatRoom);
        }

        public void Run(Server server)
        {
            // Start an asynchronous socket to listen for connections.  
            logger.LogDebug("Waiting for a request ...");

            while (!networkModule.HasDataAvailable)
            {
                Thread.Sleep(1000);
            }

            HandleHttpRequest(networkModule.CurrentRequest);
        }

        public void Start()
        {

        }

        public void Stop()
        {
            networkModule.Disconnect();
        }

        public bool Connect(ConnexionInfos connexionInfos)
        {
            return networkModule.Connect(connexionInfos);
        }

        private void HandleHttpRequest(HttpListenerContext httpContext)
        {
            HttpListenerRequest request = httpContext.Request;

            //Remove the first '/' character
            string uriPath = request.Url!.AbsolutePath.Substring(1);

            logger.LogDebug("Request received : {0} {1}", request.HttpMethod, uriPath);

            IEnumerable<IHttpRequestHandler> possibleHttpRequestHandlers = httpHandlersCache
                .Where(pair => uriPath.StartsWith(pair.Key, StringComparison.InvariantCultureIgnoreCase) && (uriPath.Length == pair.Key.Length || (uriPath.Length > pair.Key.Length && uriPath[pair.Key.Length] == '/')))
                .Select(pair => pair.Value);
            if (possibleHttpRequestHandlers.Any())
            {
                IHttpRequestHandler httpRequestHandler = possibleHttpRequestHandlers.First();
                httpRequestHandler.HandleHttpRequest(httpContext);
            }
            else
            {
                HandleRequestDefault(httpContext);
            }
        }

        private void HandleRequestDefault(HttpListenerContext context)
        {
            HttpListenerRequest request = context.Request;

            HttpListenerResponse response = context.Response;

            //Remove the first '/' character
            string uriPath = request.Url!.AbsolutePath.Substring(1);
            string osPath = !string.IsNullOrEmpty(uriPath) ? uriPath.Replace('/', Path.DirectorySeparatorChar) : "index.html";

            Stream output = response.OutputStream;

            byte[] bodyBytes;

            if (new HttpMethod(request.HttpMethod) == HttpMethod.Get && File.Exists(Path.Combine("res", osPath)))
            {
                bodyBytes = File.ReadAllBytes(Path.Combine("res", osPath));
                response.StatusCode = (int)HttpStatusCode.OK;
            }
            else
            {
                string text = "Welcome on MySharpChat server.";
                text += Environment.NewLine;
                text += $"No data at {uriPath}";
                response.StatusCode = (int)HttpStatusCode.NotFound;

                bodyBytes = Encoding.UTF8.GetBytes(text);
            }

            response.ContentLength64 = bodyBytes.Length;
            output.Write(bodyBytes);
            output.Close();
        }
    }
}
