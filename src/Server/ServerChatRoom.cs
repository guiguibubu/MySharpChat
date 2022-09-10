using MySharpChat.Core.Http;
using MySharpChat.Core.Model;
using MySharpChat.Core.Packet;
using MySharpChat.Core.Utils.Logger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;

namespace MySharpChat.Server
{
    public class ServerChatRoom : ChatRoom, IHttpRequestHandler
    {
        private static readonly Logger logger = Logger.Factory.GetLogger<ServerChatRoom>();

        public ServerChatRoom(Guid id) : base(id)
        {
        }

        public void HandleHttpRequest(HttpListenerContext httpContext)
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
                if (IsUserConnected(userIdGuid))
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
                if (IsUserConnected(userIdGuid))
                {
                    string errorMessage = $"User with userId \"{userId}\" already connected";
                    logger.LogError(errorMessage);

                    response.StatusCode = (int)HttpStatusCode.Conflict;
                    response.Close(Encoding.ASCII.GetBytes(errorMessage), true);
                }
                else
                {
                    if (string.IsNullOrEmpty(username))
                    {
                        username = "AnonymousUser";
                    }
                    if (!IsUserNameAvailable(userIdGuid, username))
                    {
                        username = GenerateNewUsername(userIdGuid, username);
                    }
                    User newUser = new User(userIdGuid, username);
                    if (Users.Contains(userIdGuid))
                    {
                        Users[userIdGuid].User.Username = username;
                        Users[userIdGuid].Connected = true;
                    }
                    else
                    {
                        Users.Add(new UserState(newUser, true));
                    }
                    logger.LogInfo("New user connected : {0}", newUser);

                    List<PacketWrapper> responsePackets = new();
                    PacketWrapper initPacket = new PacketWrapper(Id, new UserInfoPacket(userIdGuid, username, true));
                    responsePackets.Add(initPacket);

                    foreach (ChatMessage chatMessage in Messages)
                    {
                        PacketWrapper chatPacket = new PacketWrapper(Id, new ChatPacket(chatMessage));
                        responsePackets.Add(chatPacket);
                    }

                    string packetSerialized = PacketSerializer.Serialize(responsePackets);

                    response.ContentType = MediaTypeNames.Application.Json;
                    response.StatusCode = (int)HttpStatusCode.OK;
                    response.Close(Encoding.ASCII.GetBytes(packetSerialized), true);
                }
            }
            else if (request.HttpMethod == HttpMethod.Delete.ToString())
            {
                if (IsUserConnected(userIdGuid))
                {
                    logger.LogInfo("Disconnection of : {0}", Users[userIdGuid].User);
                    Users[userIdGuid].Connected = false;
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
                return;
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
            else if (request.HttpMethod == HttpMethod.Get.ToString())
            {
                HandleGetMessageRequest(httpContext);
            }
            else
            {
                string errorMessage = $"Message request must use HTTP POST or GET method";
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

            if (!Users.Contains(userIdGuid))
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
                List<PacketWrapper> packets = PacketSerializer.Deserialize(requestBody);
                User user = Users[userIdGuid].User;
                foreach (PacketWrapper packet in packets)
                {
                    if (packet.Package is ChatPacket chatPacket)
                    {
                        ChatMessage message = chatPacket.ChatMessage;
                        logger.LogInfo("Message received from {0} => {1}", user, message.Message);
                        Messages.Add(message);
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

        private void HandleGetMessageRequest(HttpListenerContext httpContext)
        {
            HttpListenerRequest request = httpContext.Request;

            HttpListenerResponse response = httpContext.Response;

            string? userId = request.QueryString["userId"];
            string? messageId = request.QueryString["messageId"];
            if (request.HttpMethod != HttpMethod.Get.ToString())
            {
                string errorMessage = "Get Message request must use HTTP GET method";
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

            if (!Users.Contains(userIdGuid))
            {
                string errorMessage = $"User with userId \"{userId}\" must be connected before reading any message";
                logger.LogError(errorMessage);

                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Close(Encoding.ASCII.GetBytes(errorMessage), true);
                return;
            }

            List<PacketWrapper> packets = new();
            IReadOnlyCollection<ChatMessage> messagesToSend;
            if (string.IsNullOrEmpty(messageId))
            {
                messagesToSend = Messages;
            }
            else
            {
                ChatMessage lastMessageReceived = Messages[Guid.Parse(messageId)];
                List<ChatMessage> messageOrdered = Messages.OrderByDescending(chat => chat.Date).ToList();
                int indexLastMessage = messageOrdered.IndexOf(lastMessageReceived);
                messagesToSend = messageOrdered.GetRange(0, indexLastMessage);
            }
            foreach (ChatMessage message in messagesToSend)
            {
                PacketWrapper packet = new PacketWrapper(Id, new ChatPacket(message));
                packets.Add(packet);
            }
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

            if (!Users.Contains(userIdGuid))
            {
                string errorMessage = $"User with userId \"{userId}\" must be connected before reading any info";
                logger.LogError(errorMessage);

                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Close(Encoding.ASCII.GetBytes(errorMessage), true);
                return;
            }

            List<PacketWrapper> packets = new();
            foreach (UserState userState in Users)
            {
                PacketWrapper packet = new PacketWrapper(Id, new UserInfoPacket(userState.User, userState.Connected));
                packets.Add(packet);
            }
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

            if (!Users.Contains(userIdGuid))
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
                List<PacketWrapper> packets = PacketSerializer.Deserialize(requestBody);
                foreach (PacketWrapper packet in packets)
                {
                    if (packet.Package is UserInfoPacket userPacket)
                    {
                        newUsername = userPacket.User.Username;
                    }
                }
                if (string.IsNullOrEmpty(newUsername) || IsUserNameAvailable(userIdGuid, newUsername))
                {
                    if (!string.IsNullOrEmpty(newUsername))
                    {
                        User user = Users[userIdGuid].User;
                        logger.LogInfo("Username change to {1} for {0}", user, newUsername);
                        user.Username = newUsername;
                    }
                    response.StatusCode = (int)HttpStatusCode.OK;
                    response.Close();
                }
                else
                {
                    string errorMessage = $"This username is already used : \"{newUsername}\"";
                    logger.LogError(errorMessage);

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

        private bool IsUserNameAvailable(Guid userId, string username)
        {
            return !Users.Where(user => user.Id != userId).Where(user => user.Connected).Select(user => user.User.Username).Contains(username);
        }

        private string GenerateNewUsername(Guid userId, string currentUsername)
        {
            string newUsername = currentUsername;
            int usernameSuffix = 1;
            while (!IsUserNameAvailable(userId, newUsername))
            {
                newUsername = currentUsername + "_" + usernameSuffix;
                usernameSuffix++;
            }
            return newUsername;
        }

        private bool IsUserConnected(Guid userId)
        {
            return Users.Contains(userId) && Users[userId].Connected;
        }
    }
}
