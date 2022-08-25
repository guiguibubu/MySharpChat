using MySharpChat.Core.Http;
using MySharpChat.Core.Packet;
using MySharpChat.Core.NetworkModule;
using MySharpChat.Core.Utils.Logger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MySharpChat.Server
{
    public class ChatSession
    {
        private static readonly Logger logger = Logger.Factory.GetLogger<ChatSession>();

        private readonly ChatSessionNetworkModule networkModule;
        public ChatSessionNetworkModule NetworkModule => networkModule;

        public event Action<ChatSession> OnSessionInitializedCallback = (ChatSession session) => { };
        public event Action<ChatSession> OnSessionFinishedCallback = (ChatSession session) => { };
        public event Action<ChatSession, PacketWrapper> OnBroadcastCallback = (ChatSession session, PacketWrapper packet) => { };
        public event Action<ChatSession, string> OnUsernameChangeCallback = (ChatSession session, string oldUsername) => { };

        public Guid ClientId { get; private set; } = Guid.NewGuid();
        public User User { get; set; } = new User();

        public bool Initialized { get; private set; } = false;

        public ChatSession(TcpClient? tcpClient)
        {
            networkModule = new ChatSessionNetworkModule(tcpClient);
        }

        public void Start(Guid serverId)
        {
            string remoteEP = networkModule.RemoteEndPoint;
            logger.LogInfo("Connection accepted. Initialize session with {0}", remoteEP);

            if (Thread.CurrentThread.Name == null)
            {
                Thread.CurrentThread.Name = $"WorkingThread{remoteEP}";
            }

            if (Initialize(serverId))
            {
                Run();

                logger.LogInfo("Connection lost. Session with {0} finished", remoteEP);

                Initialized = false;
                OnSessionFinishedCallback(this);
            }
            else
            {
                logger.LogWarning("Failed to initialize session with {0}", remoteEP);
            }

            networkModule.Disconnect();
        }

        private enum InitializeState
        {
            FirstPass,
            WaitingResponse,
            HttpRequest
        }

        private bool Initialize(Guid serverId)
        {
            InitializeState state = InitializeState.FirstPass;
            Stopwatch stopwatch = Stopwatch.StartNew();
            bool timeout = false;
            while (networkModule.IsConnected() && !Initialized && state != InitializeState.HttpRequest && !timeout)
            {
                if (state == InitializeState.FirstPass)
                {
                    if (networkModule.HasDataAvailable)
                    {
                        string content = networkModule.ReadRaw(TimeSpan.FromSeconds(1));
                        if (HttpParser.TryParseHttpRequest(content, out _))
                        {
                            state = InitializeState.HttpRequest;
                            logger.LogError("Http request. Direct chat not allowed: \"{0}\"", content);
                            HandleHttpRequest(content);
                        }
                        else
                        {
                            logger.LogWarning("Session must be initilized before handling packets");
                        }
                    }
                    else
                    {
                        logger.LogDebug("Send sessionId to client");

                        ClientId = Guid.NewGuid();
                        ClientInitialisationPacket connectInitPacket = new ClientInitialisationPacket(ClientId);
                        networkModule.Send(new PacketWrapper(serverId, connectInitPacket));
                        state = InitializeState.WaitingResponse;
                    }
                }
                else if (state == InitializeState.WaitingResponse)
                {
                    if (networkModule.HasDataAvailable)
                    {
                        string content = networkModule.ReadRaw(TimeSpan.FromSeconds(1));
                        if (PacketSerializer.TryDeserialize(content, out List<PacketWrapper> packets))
                        {
                            foreach (PacketWrapper packet in packets)
                            {
                                if (packet.Package is ClientInitialisationPacket initPackage)
                                {
                                    HandleInitPacket(initPackage);
                                }
                                else
                                {
                                    logger.LogWarning("Session must be initialized before handling any other packets. (session with {0})", networkModule.RemoteEndPoint);
                                }
                            }
                        }
                    }
                }
                timeout = stopwatch.Elapsed > TimeSpan.FromSeconds(30);
            }

            return Initialized;
        }

        private void Run()
        {
            while (networkModule.IsConnected())
            {
                if (networkModule.HasDataAvailable)
                {
                    string content = networkModule.ReadRaw(TimeSpan.FromSeconds(1));

                    if (PacketSerializer.TryDeserialize(content, out List<PacketWrapper> packets))
                    {
                        foreach (PacketWrapper packet in packets)
                        {
                            if (packet.Package is ClientInitialisationPacket initPackage)
                            {
                                HandleInitPacket(initPackage);
                            }
                            else if (packet.Package is ChatPacket package)
                            {
                                HandleChatPacket(package);
                            }
                        }
                    }
                    else if (HttpParser.TryParseHttpRequest(content, out _))
                    {
                        logger.LogError("Http request. Direct chat not allowed: \"{0}\"", content);
                        HandleHttpRequest(content);
                    }
                    else
                    {
                        logger.LogError("Data received but with the wrong format: \"{0}\"", content);
                    }
                }
            }
        }

        private void HandleInitPacket(ClientInitialisationPacket initPackage)
        {
            bool isClientInitialized = !string.IsNullOrEmpty(User.Username);
            if (isClientInitialized)
            {
                string oldUsername = User.Username;
                User.Username = initPackage.Username;
                logger.LogInfo("Username changed {0} -> {1}", oldUsername, User.Username);
                OnUsernameChangeCallback(this, oldUsername);
            }
            else
            {
                User.Username = initPackage.Username;
                Initialized = true;
                logger.LogInfo("Session initialized with {0}", networkModule.RemoteEndPoint);
                OnSessionInitializedCallback(this);
            }
        }

        private void HandleChatPacket(ChatPacket chatPacket)
        {
            string content = chatPacket.Message;

            if (!string.IsNullOrEmpty(content))
            {
                ChatPacket newChatPacket = new ChatPacket(User.Username + ": " + content);
                PacketWrapper packet = new PacketWrapper(ClientId, newChatPacket);
                OnBroadcastCallback(this, packet);
            }
        }


        private void HandleHttpRequest(string httpContent)
        {
            //TODO: Add a real ASP server to handle HTTP/WED requests. REST API ?
            if (HttpParser.TryParseHttpRequest(httpContent, out HttpRequestMessage? httpRequestMessage))
            {
                string text = "Welcome on MySharpChat server.";
                if (!string.Equals(httpRequestMessage!.RequestUri, "/"))
                {
                    text += Environment.NewLine;
                    text += $"No data at {httpRequestMessage.RequestUri}";
                }
                HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
                response.Content = new StringContent(text);
                string httpResponse = HttpParser.ToString(response).Result;
                networkModule.SendRaw(httpResponse);
            }
        }
    }
}
