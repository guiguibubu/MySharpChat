using MySharpChat.Core.Packet;
using MySharpChat.Core.SocketModule;
using MySharpChat.Core.Utils;
using MySharpChat.Core.Utils.Logger;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace MySharpChat.Server
{
    public class ChatSessionNetworkModule : INetworkModule
    {
        private static readonly Logger logger = Logger.Factory.GetLogger<ChatSessionNetworkModule>();

        private readonly Socket? m_socket = null;
        public Socket Socket { get { return m_socket!; } }

        public ChatSessionNetworkModule(Socket? socket)
        {
            if (socket == null)
                throw new ArgumentNullException(nameof(socket));

            m_socket = socket;
        }

        public string LocalEndPoint
        {
            get
            {
                if (m_socket != null && m_socket.LocalEndPoint != null)
                    return m_socket.LocalEndPoint.ToString() ?? string.Empty;
                else
                    return string.Empty;
            }
        }

        public string RemoteEndPoint
        {
            get
            {
                if (m_socket != null && m_socket.RemoteEndPoint != null)
                    return m_socket.RemoteEndPoint.ToString() ?? string.Empty;
                else
                    return string.Empty;
            }
        }

        public bool HasDataAvailable => m_socket != null && m_socket.Available > 0;

        public bool Connect(IPEndPoint remoteEP, int timeoutMs = Timeout.Infinite)
        {
            throw new NotImplementedException("ChatRoom can't connect to a client");
        }

        public bool Connect(ConnexionInfos connexionInfos)
        {
            throw new NotImplementedException("ChatRoom can't connect to a client");
        }

        public Task<bool> ConnectAsync(IPEndPoint remoteEP, int timeoutMs = Timeout.Infinite)
        {
            throw new NotImplementedException("ChatRoom can't connect to a client");
        }

        public Task<bool> ConnectAsync(ConnexionInfos connexionInfos)
        {
            throw new NotImplementedException("ChatRoom can't connect to a client");
        }

        public void Disconnect()
        {
            if (m_socket != null)
            {
                SocketUtils.ShutdownSocket(m_socket);
            }
        }

        public bool IsConnected()
        {
            return m_socket != null && SocketUtils.IsConnected(m_socket);
        }

        public void Send(PacketWrapper? packet)
        {
            if (packet == null)
                throw new ArgumentNullException(nameof(packet));

            string content = PacketSerializer.Serialize(packet);
            SendRaw(content);
        }

        public List<PacketWrapper> Read(TimeSpan timeoutSpan)
        {
            if (m_socket == null)
                throw new ArgumentException("NetworkModule not initialized");

            string content = ReadRaw(timeoutSpan);

            return PacketSerializer.Deserialize(content);
        }

        public void SendRaw(string? text)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentNullException(nameof(text));

            SocketUtils.Send(m_socket, text, this);
        }

        public string ReadRaw(TimeSpan timeoutSpan)
        {
            if(m_socket == null)
                return string.Empty;

            using (CancellationTokenSource cancelSource = new CancellationTokenSource())
            {
                CancellationToken cancelToken = cancelSource.Token;
                Task<string> readTask = ReadAsyncImpl(cancelToken);

                bool timeout = true;
                try
                {
                    timeout = !readTask.Wait(timeoutSpan);
                }
                catch (OperationCanceledException)
                {
                    timeout = true;
                }

                if (!timeout)
                {
                    try
                    {
                        string content = readTask.Result;
                        logger.LogDebug(string.Format("Read {0} bytes from socket. Data :{1}", content.Length, content));
                        return content;
                    }
                    catch (AggregateException e)
                    {
                        logger.LogError(e, "Fail to read from {0}", m_socket.RemoteEndPoint);
                        return string.Empty;
                    }
                }
                else
                {
                    cancelSource.Cancel();
                    logger.LogDebug("Reading timeout reached. Nothing received from {0} after {1} ms", m_socket.RemoteEndPoint, timeoutSpan);
                    return string.Empty;
                }
            }
        }

        private Task<string> ReadAsyncImpl(CancellationToken cancelToken = default)
        {
            return SocketUtils.ReadAsync(m_socket, this, cancelToken);
        }
    }
}
