using MySharpChat.Core.Packet;
using MySharpChat.Core.SocketModule;
using MySharpChat.Core.Utils;
using MySharpChat.Core.Utils.Logger;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MySharpChat.Server
{
    public class ChatSessionNetworkModule : INetworkModule
    {
        private static readonly Logger logger = Logger.Factory.GetLogger<ChatSessionNetworkModule>();

        private readonly TcpClient m_tcpClient;
        public TcpClient TcpClient { get { return m_tcpClient; } }

        public ChatSessionNetworkModule(TcpClient? tcpClient)
        {
            if (tcpClient == null)
                throw new ArgumentNullException(nameof(tcpClient));

            m_tcpClient = tcpClient;
        }

        public string LocalEndPoint
        {
            get
            {
                if (m_tcpClient != null && m_tcpClient.Client.LocalEndPoint != null)
                    return m_tcpClient.Client.LocalEndPoint.ToString() ?? string.Empty;
                else
                    return string.Empty;
            }
        }

        public string RemoteEndPoint
        {
            get
            {
                if (m_tcpClient != null && m_tcpClient.Client.RemoteEndPoint != null)
                    return m_tcpClient.Client.RemoteEndPoint.ToString() ?? string.Empty;
                else
                    return string.Empty;
            }
        }

        public bool HasDataAvailable => m_tcpClient != null && m_tcpClient.Available > 0;

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
            if (m_tcpClient != null)
            {
                logger.LogInfo("Disconnection of Session Network Module");
                m_tcpClient.Close();
            }
        }

        public bool IsConnected()
        {
            bool isConnected = m_tcpClient != null && m_tcpClient.Connected && SocketUtils.IsConnected(m_tcpClient.Client);
            return isConnected;
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
            if (m_tcpClient == null)
                throw new ArgumentException("NetworkModule not initialized");

            string content = ReadRaw(timeoutSpan);

            return PacketSerializer.Deserialize(content);
        }

        public void SendRaw(string? text)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentNullException(nameof(text));

            NetworkStream stream = m_tcpClient.GetStream();
            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Encoding.ASCII.GetBytes(text);
            stream.Write(byteData);

            logger.LogInfo("Request send : {0}", text);
        }

        public string ReadRaw(TimeSpan timeoutSpan)
        {
            if (m_tcpClient == null)
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
                        logger.LogError(e, "Fail to read from {0}", m_tcpClient.Client.RemoteEndPoint);
                        return string.Empty;
                    }
                }
                else
                {
                    cancelSource.Cancel();
                    logger.LogDebug("Reading timeout reached. Nothing received from {0} after {1} ms", m_tcpClient.Client.RemoteEndPoint, timeoutSpan);
                    return string.Empty;
                }
            }
        }

        private Task<string> ReadAsyncImpl(CancellationToken cancelToken = default)
        {
            return Task.Factory.StartNew(
                () =>
                {
                    string content = string.Empty;
                    try {
                        NetworkStream stream = m_tcpClient.GetStream();
                        int bytesRead;
                        // Size of receive buffer.  
                        const int BUFFER_SIZE = 256;
                        // Receive buffer.  
                        byte[] buffer = new byte[BUFFER_SIZE];
                        // Received data string.  
                        StringBuilder dataStringBuilder = new StringBuilder();

                        while (stream.DataAvailable && (bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0 && !cancelToken.IsCancellationRequested)
                        {
                            // There  might be more data, so store the data received so far.
                            string dataStr = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                            dataStringBuilder.Append(dataStr);
                        }
                        content = dataStringBuilder.ToString();
                    }
                    catch (OperationCanceledException) { }
                    return content;
                }
            , cancelToken);
        }
    }
}
