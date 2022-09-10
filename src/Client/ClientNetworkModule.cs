using MySharpChat.Core.Packet;
using MySharpChat.Core.NetworkModule;
using MySharpChat.Core.Utils;
using MySharpChat.Core.Utils.Logger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using MySharpChat.Core.Http;
using MySharpChat.Core.Model;
using MySharpChat.Client.Utils;

namespace MySharpChat.Client
{
    public class ClientNetworkModule : IClientNetworkModule
    {
        private static readonly Logger logger = Logger.Factory.GetLogger<ClientNetworkModule>();

        private readonly IClientImpl _client;

        public Guid? ServerId { get; private set; } = null;
        public Uri? ServerUri { get; private set; } = null;
        public Uri? ChatUri { get; private set; } = null;
        private readonly HttpClient m_httpClient = new HttpClient();

        public ClientNetworkModule(IClientImpl client)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            _client = client;
        }

        private CancellationTokenSource _cancellationSource = new CancellationTokenSource();
        private Task? _statusUpdateTask = null;
        private readonly Queue<PacketWrapper> packetsQueue = new();

        public bool HasDataAvailable => packetsQueue.Any();

        public PacketWrapper CurrentData => packetsQueue.Dequeue();

        public bool Connect(IPEndPoint remoteEP, int timeoutMs = Timeout.Infinite)
        {
            if (IsConnected())
                throw new InvalidOperationException("You are already connected. Disconnect before connection");

            UriBuilder serverUriBuilder = new UriBuilder("http", remoteEP.Address.ToString(), remoteEP.Port);
            ServerUri = serverUriBuilder.Uri;

            UriBuilder chatUriBuilder = new UriBuilder(ServerUri);
            chatUriBuilder.Path = "chat";
            ChatUri = chatUriBuilder.Uri;

            User localUser = _client.LocalUser;

            UriBuilder requestUriBuilder = new UriBuilder(ChatUri);
            requestUriBuilder.Path += "/connect";
            requestUriBuilder.Query = $"userId={localUser.Id}";
            requestUriBuilder.Query += $"&user={localUser.Username}";
            HttpSendRequestContext httpContext = HttpSendRequestContext.Post(requestUriBuilder.Uri);
            HttpResponseMessage httpResponseMessage = Send(httpContext, null).Result!;
            string responseContent = httpResponseMessage.Content.ReadAsStringAsync().Result;

            bool isConnected = IsConnected();

            if (isConnected && PacketSerializer.TryDeserialize(responseContent, out List<PacketWrapper> packets))
            {
                packets.ForEach(packet => packetsQueue.Enqueue(packet));
            }

            Stopwatch stopwatch = Stopwatch.StartNew();
            int attempt = 0;
            bool timeout = stopwatch.ElapsedMilliseconds > timeoutMs;
            bool attemptConnection = !isConnected && !timeout;
            while (attemptConnection)
            {
                attempt++;

                // Connect to the remote endpoint.  
                Task<HttpResponseMessage?> connectTask = Send(httpContext, null);

                try
                {
                    timeout = !connectTask.Wait(Math.Max(timeoutMs - Convert.ToInt32(stopwatch.ElapsedMilliseconds), 0));

                    isConnected = IsConnected();

                    if (!timeout)
                    {
                        httpResponseMessage = connectTask.Result!;
                        responseContent = httpResponseMessage.Content.ReadAsStringAsync().Result;

                        if (isConnected && PacketSerializer.TryDeserialize(responseContent, out packets))
                        {
                            packets.ForEach(packet => packetsQueue.Enqueue(packet));
                        }
                    }

                    attemptConnection = !isConnected && !timeout;
                }
                catch (AggregateException)
                {
                    isConnected = false;
                    attemptConnection = false;
                }
            }

            if (isConnected)
            {
                StartStatusUpdater();
                _client.ChatRoom = new ChatRoom(packetsQueue.Peek().SourceId);
            }

            return isConnected;
        }

        public bool Connect(ConnexionInfos connexionInfos)
        {
            ConnexionInfos.Data? connexionData = connexionInfos.Remote;
            if (connexionData == null)
                throw new ArgumentException(nameof(connexionInfos.Remote));

            IPEndPoint remoteEP = NetworkUtils.CreateEndPoint(connexionData);

            const int CONNECTION_TIMEOUT_MS = 5000;

            Stopwatch stopwatch = Stopwatch.StartNew();
            bool isConnected = Connect(remoteEP, CONNECTION_TIMEOUT_MS);
            bool timeout = stopwatch.ElapsedMilliseconds > CONNECTION_TIMEOUT_MS;

            if (isConnected)
            {
                logger.LogInfo("Connection success to {0} : {1}:{2}", connexionData.Hostname, connexionData.Ip, connexionData.Port);
            }
            else
            {
                if (timeout)
                {
                    logger.LogError("Connection timeout ! Fail connection in {0} ms", CONNECTION_TIMEOUT_MS);
                }
                logger.LogError("Connection fail to {0} : {1}:{2}", connexionData.Hostname, connexionData.Ip, connexionData.Port);
            }

            return isConnected;
        }

        public Task<bool> ConnectAsync(ConnexionInfos connexionInfos) { return Task.Run(() => Connect(connexionInfos)); }

        public Task<bool> ConnectAsync(IPEndPoint remoteEP, int timeoutMs = Timeout.Infinite) { return Task.Run(() => Connect(remoteEP, timeoutMs)); }

        public void Disconnect()
        {
            if (IsConnected())
            {
                logger.LogInfo("Disconnection of Network Module");
                UriBuilder requestUriBuilder = new UriBuilder(ChatUri!);
                requestUriBuilder.Path += "/connect";
                requestUriBuilder.Query = $"userId={_client.LocalUser.Id}";
                Send(HttpSendRequestContext.Delete(requestUriBuilder.Uri), null).Wait();
                StopStatusUpdater();
            }
        }

        public bool IsConnected()
        {
            if (ServerUri == null 
                || ChatUri == null)
                return false;

            UriBuilder requestUriBuilder = new UriBuilder(ChatUri!);
            requestUriBuilder.Path += "/connect";
            requestUriBuilder.Query = $"userId={_client.LocalUser.Id}";
            HttpResponseMessage httpResponseMessage = ((IClientNetworkModule)this).Read(HttpReadRequestContext.Get(requestUriBuilder.Uri))!;
            return httpResponseMessage.IsSuccessStatusCode;
        }

        public Task<HttpResponseMessage?> Send(HttpSendRequestContext context, PacketWrapper? packet)
        {
            string content = packet != null ? PacketSerializer.Serialize(packet) : "";
            logger.LogInfo("Request send : {0}", content);
            return NetworkUtils.SendAsync(m_httpClient, context, content);
        }

        public HttpResponseMessage? Read(HttpReadRequestContext context, TimeSpan timeoutSpan)
        {
            HttpResponseMessage? httpResponseMessage = null;

            using (CancellationTokenSource cancelSource = new CancellationTokenSource())
            {
                CancellationToken cancelToken = cancelSource.Token;
                Task<HttpResponseMessage?> readTask = NetworkUtils.ReadAsync(m_httpClient, context, cancelToken);
                bool timeout = !readTask.Wait(timeoutSpan);
                if (!timeout)
                {
                    httpResponseMessage = readTask.Result!;
                    string responseContent = httpResponseMessage.Content.ReadAsStringAsync().Result;
                    logger.LogInfo("Response received : {0}", responseContent);
                }
                else
                {
                    cancelSource.Cancel();
                    logger.LogDebug("Reading timeout reached. Nothing received from server after {0}", timeoutSpan);
                }
            }
            return httpResponseMessage;
        }

        private void StartStatusUpdater()
        {
            if (_statusUpdateTask == null)
                _statusUpdateTask = Task.Run(() =>
                {
                    while (!_cancellationSource.IsCancellationRequested)
                    {
                        StatusUpdateAction();
                    }
                }, _cancellationSource.Token);
        }

        private void StopStatusUpdater()
        {
            _cancellationSource.Cancel();
            try
            {
                _statusUpdateTask?.Wait();
            }
            catch (AggregateException e)
            {
                if (!(e.InnerException is TaskCanceledException))
                    throw;
            }
            _cancellationSource = new CancellationTokenSource();
            _statusUpdateTask = null;
        }

        private void StatusUpdateAction()
        {
            UserStatusUpdateAction();
            MessageStatusUpdateAction();
        }

        private void UserStatusUpdateAction()
        {
            UriBuilder requestUriBuilder = new UriBuilder(ChatUri!);
            requestUriBuilder.Path += "/user";
            requestUriBuilder.Query = $"userId={_client.LocalUser.Id}";
            HttpReadRequestContext httpContext = HttpReadRequestContext.Get(requestUriBuilder.Uri);
            HttpResponseMessage httpResponseMessage = ((IClientNetworkModule)this).Read(httpContext)!;
            string responseContent = httpResponseMessage.Content.ReadAsStringAsync().Result;

            if (PacketSerializer.TryDeserialize(responseContent, out List<PacketWrapper> packets))
            {
                packets.ForEach(packet => packetsQueue.Enqueue(packet));
            }
        }

        private void MessageStatusUpdateAction()
        {
            UriBuilder requestUriBuilder = new UriBuilder(ChatUri!);
            requestUriBuilder.Path += "/message";
            requestUriBuilder.Query = $"userId={_client.LocalUser.Id}";
            ChatRoom chatRoom = _client.ChatRoom!;
            if (chatRoom.Messages.Any())
                requestUriBuilder.Query += $"&messageId={chatRoom.Messages.MaxBy(message => message.Date)!.Id}";
            HttpReadRequestContext httpContext = HttpReadRequestContext.Get(requestUriBuilder.Uri);
            HttpResponseMessage httpResponseMessage = ((IClientNetworkModule)this).Read(httpContext)!;
            string responseContent = httpResponseMessage.Content.ReadAsStringAsync().Result;

            if (PacketSerializer.TryDeserialize(responseContent, out List<PacketWrapper> packets))
            {
                packets.ForEach(packet => packetsQueue.Enqueue(packet));
            }
        }
    }
}
