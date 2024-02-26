using MySharpChat.Core.Packet;
using MySharpChat.Core.NetworkModule;
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
using MySharpChat.Core.Utils.Collection;
using MySharpChat.Core.Constantes;
using MySharpChat.Core.Event;
using MySharpChat.Core.API;

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
            if (client is null)
                throw new ArgumentNullException(nameof(client));

            _client = client;
        }

        private CancellationTokenSource _cancellationSource = new CancellationTokenSource();
        private Task? _statusUpdateTask = null;
        private readonly Queue<PacketWrapper<ChatEvent>> packetsQueue = new();

        public bool HasDataAvailable => packetsQueue.Any();

        public PacketWrapper<ChatEvent>? CurrentData => packetsQueue.Dequeue();

        public bool Connect(IPEndPoint remoteEP, int timeoutMs = Timeout.Infinite)
        {
            if (IsConnected())
                throw new InvalidOperationException("You are already connected. Disconnect before connection");

            UriBuilder serverUriBuilder = new UriBuilder("http", remoteEP.Address.ToString(), remoteEP.Port);
            ServerUri = serverUriBuilder.Uri;

            UriBuilder chatUriBuilder = new UriBuilder(ServerUri);
            chatUriBuilder.Path = ApiConstantes.API_PREFIX;
            ChatUri = chatUriBuilder.Uri;

            User localUser = _client.LocalUser;

            IConnexionsApi connexionsApi = RestEase.RestClient.For<IConnexionsApi>(ServerUri);

            Stopwatch stopwatch = Stopwatch.StartNew();
            int attempt = 0;
            bool isConnected;
            bool attemptConnection;
            do
            {
                attempt++;

                // Connect to the remote endpoint.  
                Task<HttpResponseMessage> connectTask = connexionsApi.PostConnexionAsync(localUser.Id.ToString(), localUser.Username.ToString());

                try
                {
                    bool timeout = !connectTask.Wait(Math.Max(timeoutMs - Convert.ToInt32(stopwatch.ElapsedMilliseconds), 0));

                    isConnected = IsConnected();

                    if (!timeout)
                    {
                        HttpResponseMessage httpResponseMessage = connectTask.Result!;
                        string responseContent = httpResponseMessage.Content.ReadAsStringAsync().Result;

                        if (isConnected && PacketSerializer.TryDeserialize(responseContent, out IEnumerable<PacketWrapper<ChatEvent>> packets))
                        {
                            foreach (PacketWrapper<ChatEvent> packet in packets)
                            {
                                packetsQueue.Enqueue(packet);
                            }
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
            while (attemptConnection);

            if (isConnected)
            {
                StartStatusUpdater();
            }

            return isConnected;
        }

        public bool Connect(ConnexionInfos connexionInfos)
        {
            ConnexionInfos.Data? connexionData = connexionInfos.Remote;
            if (connexionData is null)
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

                IConnexionsApi connexionsApi = RestEase.RestClient.For<IConnexionsApi>(ServerUri);
                connexionsApi.DeleteConnexionAsync(_client.LocalUser.Id.ToString()).GetAwaiter().GetResult();

                StopStatusUpdater();
            }
        }

        public bool IsConnected()
        {
            if (ServerUri is null
                || ChatUri is null)
                return false;

            IConnexionsApi connexionsApi = RestEase.RestClient.For<IConnexionsApi>(ServerUri);
            HttpResponseMessage httpResponseMessage = connexionsApi.GetConnexionAsync(_client.LocalUser.Id.ToString()).GetAwaiter().GetResult();
            return httpResponseMessage.IsSuccessStatusCode;
        }

        public Task<HttpResponseMessage?> SendAsync<T>(HttpSendRequestContext context, T? packet)
        {
            string content = packet is not null ? PacketSerializer.Serialize(packet) : "";
            logger.LogInfo("Request send : {0}", content);
            return NetworkUtils.SendAsync(m_httpClient, context, content);
        }

        public Task<HttpResponseMessage?> SendAsync(HttpSendRequestContext context)
        {
            return SendAsync<object>(context, null);
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
            if (_statusUpdateTask is null)
                _statusUpdateTask = Task.Run(() =>
                {
                    Stopwatch stopwatch = new Stopwatch();
                    while (!_cancellationSource.IsCancellationRequested)
                    {
                        StatusUpdateAction();

                        stopwatch.Start();
                        while (stopwatch.Elapsed <= TimeSpan.FromSeconds(1))
                        {
                            // On attend 1 sec
                        }
                        stopwatch.Reset();
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
            //UserStatusUpdateAction();
            EventsStatusUpdateAction();
        }

        private void UserStatusUpdateAction()
        {
            IUsersApi usersApi = RestEase.RestClient.For<IUsersApi>(ServerUri);
            HttpResponseMessage httpResponseMessage = usersApi.GetUsersAsync(_client.LocalUser.Id.ToString()).GetAwaiter().GetResult();
            ReadEvents(httpResponseMessage);
        }

        private void EventsStatusUpdateAction()
        {
            IEventsApi eventsApi = RestEase.RestClient.For<IEventsApi>(ServerUri);
            ChatEventCollection chatEvents = _client.ChatEvents;
            string? userId = _client.LocalUser.Id.ToString();
            string? lastId = chatEvents.Any() ? chatEvents.MaxBy(chatEvent => chatEvent.Date)!.Id.ToString() : null;
            HttpResponseMessage httpResponseMessage = eventsApi.GetEventsAsync(userId, lastId).GetAwaiter().GetResult();
            ReadEvents(httpResponseMessage);
        }

        private void ReadEvents(HttpResponseMessage httpResponseMessage)
        {
            string responseContent = httpResponseMessage.Content.ReadAsStringAsync().Result;

            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                return;
            }
            if (PacketSerializer.TryDeserialize(responseContent, out IEnumerable<PacketWrapper<ChatEvent>> packets))
            {
                foreach (PacketWrapper<ChatEvent> packet in packets)
                {
                    packetsQueue.Enqueue(packet);
                }
            }
        }
    }
}
