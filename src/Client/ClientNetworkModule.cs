using MySharpChat.Core.Packet;
using MySharpChat.Core.SocketModule;
using MySharpChat.Core.Utils;
using MySharpChat.Core.Utils.Logger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MySharpChat.Client
{
    public class ClientNetworkModule : INetworkModule
    {
        private static readonly Logger logger = Logger.Factory.GetLogger<ClientNetworkModule>();

        private readonly IClientImpl _client;

        private readonly TcpClient m_tcpClient = new TcpClient(AddressFamily.InterNetwork);

        public ClientNetworkModule(IClientImpl client)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            _client = client;
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
            if (m_tcpClient.Connected)
                throw new InvalidOperationException("You are already connected. Disconnect before connection");

            m_tcpClient.Connect(remoteEP);
            bool isConnected = m_tcpClient.Connected;
            Stopwatch stopwatch = Stopwatch.StartNew();
            int attempt = 0;
            bool timeout = stopwatch.ElapsedMilliseconds > timeoutMs;
            bool attemptConnection = !isConnected && !timeout;
            while (attemptConnection)
            {
                attempt++;

                // Connect to the remote endpoint.  
                Task connectTask = m_tcpClient!.ConnectAsync(remoteEP);

                try
                {
                    timeout = !connectTask.Wait(Math.Max(timeoutMs - Convert.ToInt32(stopwatch.ElapsedMilliseconds), 0));
                    isConnected = m_tcpClient.Connected;
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
                m_tcpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                m_tcpClient.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, (int)TimeSpan.FromHours(2).TotalMilliseconds);
                m_tcpClient.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, (int)TimeSpan.FromSeconds(1).TotalMilliseconds);
                m_tcpClient.Client.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, 10);
            }

            return isConnected;
        }

        public bool Connect(ConnexionInfos connexionInfos)
        {
            ConnexionInfos.Data? connexionData = connexionInfos.Remote;
            if (connexionData == null)
                throw new ArgumentException(nameof(connexionInfos.Remote));

            IPEndPoint remoteEP = SocketUtils.CreateEndPoint(connexionData);

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
            if (m_tcpClient != null)
            {
                logger.LogInfo("Disconnection of Network Module");
                m_tcpClient.Close();
            }
        }

        public bool IsConnected()
        {
            return m_tcpClient != null && m_tcpClient.Connected && SocketUtils.IsConnected(m_tcpClient.Client);
        }

        public void Send(PacketWrapper? packet)
        {
            if (packet == null)
                throw new ArgumentNullException(nameof(packet));

            string content = PacketSerializer.Serialize(packet);
            SendImpl(content);
        }

        public List<PacketWrapper> Read(TimeSpan timeoutSpan)
        {
            if (!m_tcpClient.Connected)
                throw new ArgumentException("NetworkModule not initialized");

            string data = ReadImpl(timeoutSpan);

            return PacketSerializer.Deserialize(data);
        }

        private void SendImpl(string? text)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentNullException(nameof(text));

            NetworkStream stream = m_tcpClient.GetStream();
            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Encoding.ASCII.GetBytes(text);
            stream.Write(byteData);

            logger.LogInfo("Request send : {0}", text);
        }

        private string ReadImpl(TimeSpan timeoutSpan)
        {
            using (CancellationTokenSource cancelSource = new CancellationTokenSource())
            {
                CancellationToken cancelToken = cancelSource.Token;
                Task<string> readTask = ReadAsyncImpl(cancelToken);

                bool timeout = !readTask.Wait(timeoutSpan);

                if (!timeout)
                {
                    string text = readTask.Result;

                    logger.LogInfo("Response received : {0}", text);
                    return text;
                }
                else
                {
                    cancelSource.Cancel();
                    logger.LogDebug("Reading timeout reached. Nothing received from server after {0}", timeoutSpan);
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
                    try
                    {
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
