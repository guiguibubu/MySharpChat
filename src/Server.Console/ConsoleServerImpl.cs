using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading;
using MySharpChat.Core.Http;
using MySharpChat.Core.Packet;
using MySharpChat.Core.Utils.Logger;
using MySharpChat.Server.Utils;

namespace MySharpChat.Server
{
    internal class ConsoleServerImpl : IServerImpl, IHttpRequestHandler
    {
        private static readonly Logger logger = Logger.Factory.GetLogger<ConsoleServerImpl>();

        private readonly IServerNetworkModule networkModule;
        public IServerNetworkModule NetworkModule => networkModule;

        public ServerChatRoom ChatRoom { get; private set; }

        public ConsoleServerImpl()
        {
            networkModule = new ServerNetworkModule();
            ChatRoom = new ServerChatRoom(Guid.NewGuid());
        }

        public void Run(Server server)
        {
            // Start an asynchronous socket to listen for connections.  
            logger.LogDebug("Waiting for a request ...");

            while (!networkModule.HasDataAvailable)
            {
                Thread.Sleep(1000);
            }

            HandleHttpRequest(networkModule.CurrentData);
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

        public void HandleHttpRequest(HttpListenerContext httpContext)
        {
            HttpListenerRequest request = httpContext.Request;

            //Remove the first '/' character
            string uriPath = request.Url!.AbsolutePath.Substring(1);

            logger.LogDebug("Request received : {0} {1}", request.HttpMethod, uriPath);

            bool isChatRequest = uriPath.StartsWith("chat", StringComparison.InvariantCultureIgnoreCase) && (uriPath.Length == "chat".Length || (uriPath.Length > "chat".Length && uriPath["chat".Length] == '/'));
            if (isChatRequest)
            {
                HandleHttpRequestImpl(httpContext);
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

        public void HandleHttpRequestImpl(HttpListenerContext httpContext)
        {
            HttpListenerRequest request = httpContext.Request;

            //Remove the first '/' character
            string uriPath = request.Url!.AbsolutePath.Substring(1).Substring("chat".Length);

            if (!string.IsNullOrEmpty(uriPath))
            {
                uriPath = uriPath.Substring(1);
            }
            HttpListenerResponse response = httpContext.Response;

            if (uriPath.StartsWith("connect"))
            {
                HandleConnexionRequest(httpContext);
            }
            else if (uriPath.StartsWith("message"))
            {
                HandleMessageRequest(httpContext);
            }
            else if (uriPath.StartsWith("event"))
            {
                HandleEventsRequest(httpContext);
            }
            else if (uriPath.StartsWith("user"))
            {
                HandleUserRequest(httpContext);
            }
            else
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Close();
            }
        }

        private void HandleConnexionRequest(HttpListenerContext httpContext)
        {
            HttpListenerRequest request = httpContext.Request;

            HttpListenerResponse response = httpContext.Response;

            string? username = request.QueryString["user"];
            string? userId = request.QueryString["userId"];

            if (string.IsNullOrEmpty(userId))
            {
                string errorMessage = "Connection request must have a \"userId\" param";
                logger.LogError(errorMessage);

                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Close(Encoding.ASCII.GetBytes(errorMessage), true);
                return;
            }
            if (!Guid.TryParse(userId, out Guid userIdGuid))
            {
                string errorMessage = "Connection request parameter \"userId\" must respect GUID format";
                logger.LogError(errorMessage);

                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Close(Encoding.ASCII.GetBytes(errorMessage), true);
                return;
            }

            if (request.HttpMethod == HttpMethod.Get.ToString())
            {
                if (ChatRoom.IsUserConnected(userIdGuid))
                {
                    response.StatusCode = (int)HttpStatusCode.OK;
                    response.Close();
                }
                else
                {
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.Close();
                }
            }
            else if (request.HttpMethod == HttpMethod.Post.ToString())
            {
                if (ChatRoom.IsUserConnected(userIdGuid))
                {
                    string errorMessage = $"User with userId \"{userId}\" already connected";
                    logger.LogError(errorMessage);

                    response.StatusCode = (int)HttpStatusCode.Conflict;
                    response.Close(Encoding.ASCII.GetBytes(errorMessage), true);
                }
                else
                {
                    IEnumerable<PacketWrapper> responsePackets = ChatRoom.ConnectUser(username, userIdGuid);
                    string packetSerialized = PacketSerializer.Serialize(responsePackets);

                    response.ContentType = MediaTypeNames.Application.Json;
                    response.StatusCode = (int)HttpStatusCode.OK;
                    response.Close(Encoding.ASCII.GetBytes(packetSerialized), true);
                }
            }
            else if (request.HttpMethod == HttpMethod.Delete.ToString())
            {
                if (ChatRoom.IsUserConnected(userIdGuid))
                {
                    ChatRoom.DisconnectUser(userIdGuid);
                }
                response.StatusCode = (int)HttpStatusCode.OK;
                response.Close();
            }
            else
            {
                string errorMessage = "Connection request must use HTTP POST method or GET method";
                logger.LogError(errorMessage);

                response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                response.Close(Encoding.ASCII.GetBytes(errorMessage), true);
            }
        }

        private void HandleMessageRequest(HttpListenerContext httpContext)
        {
            HttpListenerRequest request = httpContext.Request;

            HttpListenerResponse response = httpContext.Response;

            if (request.HttpMethod == HttpMethod.Post.ToString())
            {
                HandleAddMessageRequest(httpContext);
            }
            else
            {
                string errorMessage = $"Message request must use HTTP POST method";
                logger.LogError(errorMessage);

                response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                response.Close(Encoding.ASCII.GetBytes(errorMessage), true);
            }
        }

        private void HandleAddMessageRequest(HttpListenerContext httpContext)
        {
            HttpListenerRequest request = httpContext.Request;

            HttpListenerResponse response = httpContext.Response;

            string? userId = request.QueryString["userId"];
            if (request.HttpMethod != HttpMethod.Post.ToString())
            {
                string errorMessage = "Add Message request must use HTTP POST method";
                logger.LogError(errorMessage);

                response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                response.Close(Encoding.ASCII.GetBytes(errorMessage), true);
                return;
            }
            if (string.IsNullOrEmpty(userId))
            {
                string errorMessage = "Message request must have a \"userId\" param";
                logger.LogError(errorMessage);

                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Close(Encoding.ASCII.GetBytes(errorMessage), true);
                return;
            }
            if (!Guid.TryParse(userId, out Guid userIdGuid))
            {
                string errorMessage = "Message request parameter \"userId\" must respect GUID format";
                logger.LogError(errorMessage);

                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Close(Encoding.ASCII.GetBytes(errorMessage), true);
                return;
            }

            if (!ChatRoom.IsUserConnected(userIdGuid))
            {
                string errorMessage = $"User with userId \"{userId}\" must be connected before sending any message";
                logger.LogError(errorMessage);

                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Close(Encoding.ASCII.GetBytes(errorMessage), true);
                return;
            }

            string? httpContent = request.ContentType;
            if (!string.Equals(httpContent, MediaTypeNames.Application.Json))
            {
                string errorMessage = $"Message request body must be of format : {MediaTypeNames.Application.Json}";
                logger.LogError(errorMessage);

                response.StatusCode = (int)HttpStatusCode.UnsupportedMediaType;
                response.Close(Encoding.ASCII.GetBytes(errorMessage), true);
                return;
            }

            string requestBody = new StreamReader(request.InputStream).ReadToEnd();
            if (!string.IsNullOrEmpty(requestBody))
            {
                IEnumerable<PacketWrapper> packets = PacketSerializer.Deserialize(requestBody);
                foreach (PacketWrapper packet in packets)
                {
                    if (packet.Package is ChatMessagePacket chatPacket)
                    {
                        ChatRoom.AddMessage(userIdGuid, chatPacket.ChatMessage);
                    }
                }
                response.StatusCode = (int)HttpStatusCode.OK;
                response.Close();
            }
            else
            {
                string errorMessage = $"Message request body must not be empty";
                logger.LogError(errorMessage);

                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Close(Encoding.ASCII.GetBytes(errorMessage), true);
            }
        }

        private void HandleEventsRequest(HttpListenerContext httpContext)
        {
            HttpListenerRequest request = httpContext.Request;

            HttpListenerResponse response = httpContext.Response;

            if (request.HttpMethod == HttpMethod.Get.ToString())
            {
                HandleGetEventsRequest(httpContext);
            }
            else
            {
                string errorMessage = $"Event request must use HTTP GET method";
                logger.LogError(errorMessage);

                response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                response.Close(Encoding.ASCII.GetBytes(errorMessage), true);
            }
        }

        private void HandleGetEventsRequest(HttpListenerContext httpContext)
        {
            HttpListenerRequest request = httpContext.Request;

            HttpListenerResponse response = httpContext.Response;

            string? userId = request.QueryString["userId"];
            string? lastId = request.QueryString["lastId"];
            if (request.HttpMethod != HttpMethod.Get.ToString())
            {
                string errorMessage = "Get Events request must use HTTP GET method";
                logger.LogError(errorMessage);

                response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                response.Close(Encoding.ASCII.GetBytes(errorMessage), true);
                return;
            }
            if (string.IsNullOrEmpty(userId))
            {
                string errorMessage = "Events request must have a \"userId\" param";
                logger.LogError(errorMessage);

                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Close(Encoding.ASCII.GetBytes(errorMessage), true);
                return;
            }
            if (!Guid.TryParse(userId, out Guid userIdGuid))
            {
                string errorMessage = "Events request parameter \"userId\" must respect GUID format";
                logger.LogError(errorMessage);

                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Close(Encoding.ASCII.GetBytes(errorMessage), true);
                return;
            }

            if (!ChatRoom.IsUserConnected(userIdGuid))
            {
                string errorMessage = $"User with userId \"{userId}\" must be connected before reading any event";
                logger.LogError(errorMessage);

                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Close(Encoding.ASCII.GetBytes(errorMessage), true);
                return;
            }

            IEnumerable<PacketWrapper> packets = ChatRoom.GetChatEvents(lastId);
            string responseContent = PacketSerializer.Serialize(packets);
            response.StatusCode = (int)HttpStatusCode.OK;
            response.Close(Encoding.ASCII.GetBytes(responseContent), true);
        }

        private void HandleUserRequest(HttpListenerContext httpContext)
        {
            HttpListenerRequest request = httpContext.Request;

            HttpListenerResponse response = httpContext.Response;

            if (request.HttpMethod == HttpMethod.Put.ToString())
            {
                HandlePutUserRequest(httpContext);
            }
            else if (request.HttpMethod == HttpMethod.Get.ToString())
            {
                HandleGetUserRequest(httpContext);
            }
            else
            {
                string errorMessage = "User request must use HTTP PUT or GET method";
                logger.LogError(errorMessage);

                response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                response.Close(Encoding.ASCII.GetBytes(errorMessage), true);
            }
        }

        private void HandleGetUserRequest(HttpListenerContext httpContext)
        {
            HttpListenerRequest request = httpContext.Request;

            HttpListenerResponse response = httpContext.Response;

            string? userId = request.QueryString["userId"];
            if (request.HttpMethod != HttpMethod.Get.ToString())
            {
                string errorMessage = "Get User request must use HTTP GET method";
                logger.LogError(errorMessage);

                response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                response.Close(Encoding.ASCII.GetBytes(errorMessage), true);
                return;
            }

            if (string.IsNullOrEmpty(userId))
            {
                string errorMessage = "Get User request must have a \"userId\" param";
                logger.LogError(errorMessage);

                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Close(Encoding.ASCII.GetBytes(errorMessage), true);
                return;
            }
            if (!Guid.TryParse(userId, out Guid userIdGuid))
            {
                string errorMessage = "Get User request parameter \"userId\" must respect GUID format";
                logger.LogError(errorMessage);

                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Close(Encoding.ASCII.GetBytes(errorMessage), true);
                return;
            }

            if (!ChatRoom.IsUserConnected(userIdGuid))
            {
                string errorMessage = $"User with userId \"{userId}\" must be connected before reading any info";
                logger.LogError(errorMessage);

                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Close(Encoding.ASCII.GetBytes(errorMessage), true);
                return;
            }

            IEnumerable<PacketWrapper> packets = ChatRoom.GetUsers();

            string responseContent = PacketSerializer.Serialize(packets);
            response.StatusCode = (int)HttpStatusCode.OK;
            response.Close(Encoding.ASCII.GetBytes(responseContent), true);
        }

        private void HandlePutUserRequest(HttpListenerContext httpContext)
        {
            HttpListenerRequest request = httpContext.Request;

            HttpListenerResponse response = httpContext.Response;

            string? userId = request.QueryString["userId"];
            if (request.HttpMethod != HttpMethod.Put.ToString())
            {
                string errorMessage = "User request must use HTTP PUT method";
                logger.LogError(errorMessage);

                response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                response.Close(Encoding.ASCII.GetBytes(errorMessage), true);
                return;
            }
            if (string.IsNullOrEmpty(userId))
            {
                string errorMessage = "User request must have a \"userId\" param";
                logger.LogError(errorMessage);

                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Close(Encoding.ASCII.GetBytes(errorMessage), true);
                return;
            }
            if (!Guid.TryParse(userId, out Guid userIdGuid))
            {
                string errorMessage = "User request parameter \"userId\" must respect GUID format";
                logger.LogError(errorMessage);

                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Close(Encoding.ASCII.GetBytes(errorMessage), true);
                return;
            }

            if (!ChatRoom.IsUserConnected(userIdGuid))
            {
                string errorMessage = $"User with userId \"{userId}\" must be connected before sending any message";
                logger.LogError(errorMessage);

                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Close(Encoding.ASCII.GetBytes(errorMessage), true);
                return;
            }

            string? httpContent = request.ContentType;
            if (!string.Equals(httpContent, MediaTypeNames.Application.Json))
            {
                string errorMessage = $"User request body must be of format : {MediaTypeNames.Application.Json}";
                logger.LogError(errorMessage);

                response.StatusCode = (int)HttpStatusCode.UnsupportedMediaType;
                response.Close(Encoding.ASCII.GetBytes(errorMessage), true);
                return;
            }

            string requestBody = new StreamReader(request.InputStream).ReadToEnd();
            if (!string.IsNullOrEmpty(requestBody))
            {
                string newUsername = "";
                IEnumerable<PacketWrapper> packets = PacketSerializer.Deserialize(requestBody);
                foreach (PacketWrapper packet in packets)
                {
                    if (packet.Package is UserInfoPacket userPacket)
                    {
                        newUsername = userPacket.UserState.User.Username;
                    }
                }
                if (ChatRoom.ModifyUser(userIdGuid, newUsername))
                {
                    response.StatusCode = (int)HttpStatusCode.OK;
                    response.Close();
                }
                else
                {
                    string errorMessage = $"This username is already used : \"{newUsername}\"";

                    response.StatusCode = (int)HttpStatusCode.Conflict;
                    response.Close(Encoding.ASCII.GetBytes(errorMessage), true);
                }
            }
            else
            {
                string errorMessage = $"Message request body must not be empty";
                logger.LogError(errorMessage);

                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Close(Encoding.ASCII.GetBytes(errorMessage), true);
            }
        }
    }
}
