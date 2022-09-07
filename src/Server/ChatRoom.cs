using MySharpChat.Core.Http;
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
    public class ChatRoom : IHttpRequestHandler
    {
        private static readonly Logger logger = Logger.Factory.GetLogger<ChatRoom>();

        private readonly Dictionary<string, User> m_connectedUsers = new();
        private readonly Dictionary<string, ChatMessage> m_messages = new();
        private readonly Guid _serverId;

        public ChatRoom(Guid serverId)
        {
            _serverId = serverId;
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
                HandleConnectionRequest(httpContext);
            }
            else if (uriPath.StartsWith("message"))
            {
                HandleMessageRequest(httpContext);
            }
            else
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Close();
            }
        }

        private void HandleConnectionRequest(HttpListenerContext httpContext)
        {
            HttpListenerRequest request = httpContext.Request;

            HttpListenerResponse response = httpContext.Response;

            string? username = request.QueryString["user"];
            string? userId = request.QueryString["userId"];
            if (request.HttpMethod != HttpMethod.Post.ToString())
            {
                string errorMessage = "Connection request must use HTTP POST method";
                logger.LogError(errorMessage);

                response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                response.Close(Encoding.ASCII.GetBytes(errorMessage), true);
                return;
            }
            if (string.IsNullOrEmpty(userId))
            {
                string errorMessage = "Connection request must have a \"userId\" param";
                logger.LogError(errorMessage);

                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Close(Encoding.ASCII.GetBytes(errorMessage), true);
                return;
            }
            if (!Guid.TryParse(userId, out _))
            {
                string errorMessage = "Connection request parameter \"userId\" must respect GUID format";
                logger.LogError(errorMessage);

                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Close(Encoding.ASCII.GetBytes(errorMessage), true);
                return;
            }

            if (m_connectedUsers.ContainsKey(userId))
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
                if (!IsUserNameAvailable(userId, username))
                {
                    username = GenerateNewUsername(userId, username);
                }
                m_connectedUsers.Add(userId, new User(username));

                List<PacketWrapper> responsePackets = new();
                PacketWrapper initPacket = new PacketWrapper(_serverId, new ClientInitialisationPacket(Guid.Parse(userId), username));
                responsePackets.Add(initPacket);

                foreach(ChatMessage chatMessage in m_messages.Values)
                {
                    PacketWrapper chatPacket = new PacketWrapper(_serverId, new ChatPacket(chatMessage.Message));
                    responsePackets.Add(chatPacket);
                }

                string packetSerialized = PacketSerializer.Serialize(responsePackets);

                response.ContentType = MediaTypeNames.Application.Json;
                response.StatusCode = (int)HttpStatusCode.OK;
                response.Close(Encoding.ASCII.GetBytes(packetSerialized), true);
            }
        }

        private void HandleMessageRequest(HttpListenerContext httpContext)
        {
            HttpListenerRequest request = httpContext.Request;

            HttpListenerResponse response = httpContext.Response;

            string? userId = request.QueryString["userId"];
            if (request.HttpMethod != HttpMethod.Post.ToString())
            {
                string errorMessage = "Message request must use HTTP POST method";
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
            if (!Guid.TryParse(userId, out _))
            {
                string errorMessage = "Connection request parameter \"userId\" must respect GUID format";
                logger.LogError(errorMessage);

                response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Close(Encoding.ASCII.GetBytes(errorMessage), true);
                return;
            }

            if (!m_connectedUsers.ContainsKey(userId))
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
                List<PacketWrapper> packets = PacketSerializer.Deserialize(new StreamReader(request.InputStream).ReadToEnd());
                foreach (PacketWrapper packet in packets)
                {
                    if (packet.Package is ChatPacket chatPacket)
                    {
                        m_messages.Add(userId, new ChatMessage(m_connectedUsers[userId], chatPacket.Message));
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

        private bool IsUserNameAvailable(string userId, string username)
        {
            return !m_connectedUsers.Where(pair => pair.Key == userId).Select(pair => pair.Value.Username).Contains(username);
        }

        private string GenerateNewUsername(string userId, string currentUsername)
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
    }
}
