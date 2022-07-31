using MySharpChat.Core.Http;
using MySharpChat.Core.Packet;
using MySharpChat.Core.SocketModule;
using MySharpChat.Core.Utils.Logger;
using System;
using System.Collections.Generic;
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
        public event Action<ChatSession, string> OnBroadcastCallback = (ChatSession session, string text) => { };
        public event Action<ChatSession, string> OnUsernameChangeCallback = (ChatSession session, string oldUsername) => { };

        public Guid ClientId { get; private set; } = Guid.NewGuid();
        public string ClientUsername { get; set; } = "";
        
        public ChatSession(Socket? socket)
        {
            networkModule = new ChatSessionNetworkModule(socket);
        }

        public void Start(Guid serverId)
        {
            string remoteEP = networkModule.RemoteEndPoint;
            logger.LogInfo("Connection accepted. Begin session with {0}", remoteEP);

            if (Thread.CurrentThread.Name == null)
            {
                Thread.CurrentThread.Name = $"WorkingThread{remoteEP}";
            }

            logger.LogDebug("Send sessionId to client");

            ClientId = Guid.NewGuid();
            ClientInitialisationPacket connectInitPacket = new ClientInitialisationPacket(ClientId);
            networkModule.Send(new PacketWrapper(serverId, connectInitPacket));

            Run();

            logger.LogInfo("Connection lost. Session with {0} finished", remoteEP);

            OnSessionFinishedCallback(this);

            networkModule.Disconnect();
        }

        private void Run()
        {
            while (networkModule.IsConnected())
            {
                if (networkModule.HasDataAvailable)
                {
                    List<PacketWrapper> packets = networkModule.Read(TimeSpan.FromSeconds(1));
                    foreach (PacketWrapper packet in packets)
                    {
                        if(packet.Package is ClientInitialisationPacket initPackage)
                        {
                            bool isClientInitialized = !string.IsNullOrEmpty(ClientUsername);
                            if (isClientInitialized)
                            {
                                string oldUsername = ClientUsername;
                                ClientUsername = initPackage.Username;
                                OnUsernameChangeCallback(this, oldUsername);
                            }
                            else
                            {
                                ClientUsername = initPackage.Username;
                                OnSessionInitializedCallback(this);
                            }
                        }
                        else if (packet.Package is ChatPacket package)
                        {
                            HandleChatPacket(package);
                        }
                    }
                }
            }
        }

        private void HandleChatPacket(ChatPacket chatPacket)
        {
            string content = chatPacket.Message;

            if (!string.IsNullOrEmpty(content))
            {
                // All the data has been read from the
                // client. Display it on the console.  

                logger.LogDebug(string.Format("Read {0} bytes from socket. Data :{1}", content.Length, content));

                //TODO: Add a real ASP server to handle HTTP/WED requests. REST API ?
                // Echo the data back to the client.
                if (HttpParser.TryParseHttpRequest(content, out HttpRequestMessage? httpRequestMessage))
                {
                    string text = "Welcome on MySharpChat server.";
                    if (!string.Equals(httpRequestMessage!.RequestUri, "/"))
                    {
                        text += Environment.NewLine;
                        text += $"No data at {httpRequestMessage.RequestUri}";
                    }
                    HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Content = new StringContent(text);
                    content = HttpParser.ToString(response).Result;
                }

                //TODO: Add a real logic instead of basic re-send. User Authentification ? Spawn dedicated chat servers ?
                OnBroadcastCallback(this, content);
            }
        }
    }
}
