using MySharpChat.Core.Packet;
using MySharpChat.Core.SocketModule;
using MySharpChat.Core.UI;
using MySharpChat.Core.Utils;
using MySharpChat.Core.Utils.Logger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MySharpChat.Client
{
    public class ClientNetworkModule : INetworkModule
    {
        private static readonly Logger logger = Logger.Factory.GetLogger<ClientNetworkModule>();

        private readonly IClientImpl _client;

        private Socket? m_socket = null;

        public ClientNetworkModule(IClientImpl client)
        {
            if(client == null)
                throw new ArgumentNullException(nameof(client));

            _client = client;
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

        public bool Connect(IPEndPoint remoteEP, out bool isConnected, int timeoutMs = Timeout.Infinite)
        {
            bool timeout = false;
            isConnected = false;

            Socket? socket = m_socket;

            if (socket != null)
            {
                isConnected = SocketUtils.IsConnected(socket);
                Stopwatch stopwatch = Stopwatch.StartNew();
                int attempt = 0;
                timeout = stopwatch.ElapsedMilliseconds > timeoutMs;
                bool attemptConnection = !isConnected && !timeout;
                while (attemptConnection)
                {
                    attempt++;

                    // Connect to the remote endpoint.  
                    Task connectTask = socket!.ConnectAsync(remoteEP);

                    const string prefix = "Connecting";
                    const int nbDotsMax = 3;
                    StringBuilder loadingText = new StringBuilder(prefix);
                    int nbDots = attempt % (nbDotsMax + 1);
                    for (int i = 0; i < nbDots; i++)
                        loadingText.Append(".");
                    for (int i = 0; i < nbDotsMax - nbDots; i++)
                        loadingText.Append(" ");

                    IUserInputCursorHandler cursorHandler = _client.UserInterfaceModule.CursorHandler;
                    LockTextWriter writer = _client.UserInterfaceModule.OutputWriter;
                    writer.Write(loadingText);
                    cursorHandler.MovePositionNegative(loadingText.Length, CursorUpdateMode.GraphicalOnly);

                    try
                    {
                        timeout = !connectTask.Wait(Math.Max(timeoutMs - Convert.ToInt32(stopwatch.ElapsedMilliseconds), 0));
                        isConnected = SocketUtils.IsConnected(socket);
                        attemptConnection = !isConnected && !timeout;
                    }
                    catch (AggregateException)
                    {
                        timeout = false;
                        isConnected = false;
                        attemptConnection = false;
                    }
                }
            }

            return timeout;
        }

        public bool Connect(ConnexionInfos connexionInfos)
        {
            ConnexionInfos.Data? connexionData = connexionInfos.Remote;
            if (connexionData == null)
                throw new ArgumentException(nameof(connexionInfos.Remote));

            IPEndPoint remoteEP = SocketUtils.CreateEndPoint(connexionData);

            // Create a TCP/IP socket.  
            m_socket = SocketUtils.CreateSocket(connexionData);

            const int CONNECTION_TIMEOUT_MS = 5000;

            bool timeout = Connect(remoteEP, out bool isConnected, CONNECTION_TIMEOUT_MS);

            LockTextWriter writer = _client.UserInterfaceModule.OutputWriter;

            if (isConnected)
            {
                writer.WriteLine("Connection success to {0} : {1}:{2}", connexionData.Hostname, connexionData.Ip, connexionData.Port);
            }
            else
            {
                if (timeout)
                    writer.WriteLine("Connection timeout ! Fail connection in {0} ms", CONNECTION_TIMEOUT_MS);
                writer.WriteLine("Connection fail to {0} : {1}:{2}", connexionData.Hostname, connexionData.Ip, connexionData.Port);
            }

            return isConnected;
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
            SendImpl(content);
        }

        public List<PacketWrapper> Read(TimeSpan timeoutSpan)
        {
            if (m_socket == null)
                throw new ArgumentException("NetworkModule not initialized");

            string content = ReadImpl(timeoutSpan);

            return PacketSerializer.Deserialize(content);
        }

        private void SendImpl(string? text)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentNullException(nameof(text));

            SocketUtils.Send(m_socket, text, this);
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
            return SocketUtils.ReadAsync(m_socket, this, cancelToken);
        }
    }
}
