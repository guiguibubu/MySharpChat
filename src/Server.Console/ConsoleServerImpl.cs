using MySharpChat.Core.Packet;
using MySharpChat.Core.NetworkModule;
using MySharpChat.Core.Utils.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.IO;
using System.Net.Http;
using MySharpChat.Core.Http;
using MySharpChat.Server.Utils;
using System.Net.Mime;
using MySharpChat.Core.Constantes;

namespace MySharpChat.Server.Console
{
    internal class ConsoleServerImpl : IServerImpl, IHttpRequestHandler
    {
        private static readonly Logger logger = Logger.Factory.GetLogger<ConsoleServerImpl>();

        private readonly IServerNetworkModule _networkModule;

        public ServerChatRoom ChatRoom { get; private set; }

        public ConsoleServerImpl()
        {
            _networkModule = new HttpServerNetworkModule();
            ChatRoom = new ServerChatRoom(Guid.NewGuid());
        }

        public void Run(Server server)
        {
            // Start an asynchronous socket to listen for connections.  
            logger.LogDebug("Waiting for a request ...");

            while (!_networkModule.HasDataAvailable)
            {
                Thread.Sleep(1000);
            }

            HandleHttpRequest(_networkModule.CurrentData);
        }

        public void Start()
        {
            ConnexionInfos connexionInfos = new ConnexionInfos();
            ConnexionInfos.Data data = connexionInfos.Local!;

            (IEnumerable<IPAddress> ipAddressesHost, IEnumerable<IPAddress> ipAddressesNonVirtual) = NetworkUtils.GetAvailableIpAdresses();
            data.Ip = ipAddressesHost.Intersect(ipAddressesNonVirtual).FirstOrDefault();
            if (data.Ip == null)
            {
                StringBuilder sb = new();
                sb.AppendLine("No valid ip adress available");
                sb.AppendLine("Available ip adresses Host");
                foreach (IPAddress ipAddress in ipAddressesHost)
                {
                    sb.AppendLine(string.Format("{0} ({1})", ipAddress, string.Join(",", ipAddress.AddressFamily)));

                }
                sb.AppendLine("Available ip adresses non virtual");
                foreach (IPAddress ipAddress in ipAddressesNonVirtual)
                {
                    sb.AppendLine(string.Format("{0} ({1})", ipAddress, string.Join(",", ipAddress.AddressFamily)));
                }
                logger.LogError(sb.ToString());
                throw new InvalidOperationException("No valid ip adress available");
            }

            data.Port = ConnexionInfos.DEFAULT_PORT;

            Connect(connexionInfos);
        }

        public void Stop()
        {
            _networkModule.Disconnect();
        }

        public bool Connect(ConnexionInfos connexionInfos)
        {
            return _networkModule.Connect(connexionInfos);
        }

        public void HandleHttpRequest(HttpListenerContext? httpContext)
        {
            ArgumentNullException.ThrowIfNull(httpContext);

            HttpListenerRequest request = httpContext.Request;

            //Remove the first '/' character
            string uriPath = request.Url!.AbsolutePath.Substring(1);

            logger.LogDebug("Request received : {0} {1}", request.HttpMethod, uriPath);

            bool isApiRequest = (uriPath.StartsWith(ApiConstantes.API_PREFIX, StringComparison.InvariantCultureIgnoreCase) && uriPath.Length == ApiConstantes.API_PREFIX.Length)
                || uriPath.StartsWith(ApiConstantes.API_PREFIX + '/', StringComparison.InvariantCultureIgnoreCase);
            if (isApiRequest)
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

        private void HandleHttpRequestImpl(HttpListenerContext httpContext)
        {
            HttpListenerRequest request = httpContext.Request;

            //Remove the first '/' character
            string uriPath = request.Url!.AbsolutePath.Substring(1).Substring(ApiConstantes.API_PREFIX.Length);

            if (!string.IsNullOrEmpty(uriPath))
            {
                uriPath = uriPath.Substring(1);
            }
            HttpListenerResponse response = httpContext.Response;

            if (uriPath.StartsWith(ApiConstantes.API_CONNEXION_PREFIX))
            {
                HandleConnexionRequest(httpContext);
            }
            else if (uriPath.StartsWith(ApiConstantes.API_MESSAGE_PREFIX))
            {
                HandleMessageRequest(httpContext);
            }
            else if (uriPath.StartsWith(ApiConstantes.API_EVENT_PREFIX))
            {
                HandleEventsRequest(httpContext);
            }
            else if (uriPath.StartsWith(ApiConstantes.API_USER_PREFIX))
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

            string? username = request.QueryString["username"];
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
                    ChatRoom.ConnectUser(username, userIdGuid);

                    response.StatusCode = (int)HttpStatusCode.NoContent;
                    response.Close();
                }
            }
            else if (request.HttpMethod == HttpMethod.Delete.ToString())
            {
                if (ChatRoom.IsUserConnected(userIdGuid))
                {
                    ChatRoom.DisconnectUser(userIdGuid);
                }
                response.StatusCode = (int)HttpStatusCode.NoContent;
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
            if (!string.IsNullOrEmpty(requestBody)
                && PacketSerializer.TryDeserialize(requestBody, out IEnumerable<PacketWrapper<ChatMessagePacket>> packets))
            {
                foreach (ChatMessagePacket chatPacket in packets.Where(p => p.Package is not null).Select(p => p.Package))
                {
                    ChatRoom.AddMessage(userIdGuid, chatPacket.ChatMessage);
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

            IEnumerable<ChatEventPacketWrapper> packets = ChatRoom.GetChatEventPackets(lastId);
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

            IEnumerable<PacketWrapper<UserInfoPacket>> packets = ChatRoom.GetUserPackets();

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
            if (!string.IsNullOrEmpty(requestBody)
                && PacketSerializer.TryDeserialize(requestBody, out IEnumerable<PacketWrapper<UserInfoPacket>> packets))
            {
                string newUsername = "";
                foreach (UserInfoPacket userPacket in packets.Where(p => p.Package is not null).Select(p => p.Package))
                {
                    newUsername = userPacket.UserState.User.Username;
                }
                if (ChatRoom.ModifyUser(userIdGuid, newUsername))
                {
                    response.StatusCode = (int)HttpStatusCode.NoContent;
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
