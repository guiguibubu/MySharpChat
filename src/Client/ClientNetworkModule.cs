using MySharpChat.Client.Console;
using MySharpChat.Client.Input;
using MySharpChat.Client.UI;
using MySharpChat.Core.SocketModule;
using MySharpChat.Core.Utils;
using MySharpChat.Core.Utils.Logger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MySharpChat.Client
{
    internal class ClientNetworkModule : INetworkMachine
    {
        private static readonly Logger logger = Logger.Factory.GetLogger<ClientNetworkModule>();

        private readonly ClientOutputWriter _outputWriter;

        private Socket? m_socket = null;

        public ClientNetworkModule(ClientOutputWriter outputWriter)
        {
            if(outputWriter == null)
                throw new ArgumentNullException(nameof(outputWriter));

            _outputWriter = outputWriter;
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

                    ConsoleCursorContext cursorContext = new ConsoleCursorContext();
                    int oldCursorPositionX = cursorContext.X;
                    int oldCursorPositionY = cursorContext.Y;
                    _outputWriter.Write(loadingText);
                    cursorContext.X = oldCursorPositionX;
                    cursorContext.Y = oldCursorPositionY;

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
            m_socket = SocketUtils.OpenListener(connexionData);

            const int CONNECTION_TIMEOUT_MS = 5000;

            bool timeout = Connect(remoteEP, out bool isConnected, CONNECTION_TIMEOUT_MS);

            if (isConnected)
            {
                _outputWriter.WriteLine("Connection success to {0} : {1}:{2}", connexionData.Hostname, connexionData.Ip, connexionData.Port);
            }
            else
            {
                if (timeout)
                    _outputWriter.WriteLine("Connection timeout ! Fail connection in {0} ms", CONNECTION_TIMEOUT_MS);
                _outputWriter.WriteLine("Connection fail to {0} : {1}:{2}", connexionData.Hostname, connexionData.Ip, connexionData.Port);
            }

            return isConnected;
        }

        public void Disconnect()
        {
            if (m_socket != null)
            {
                SocketUtils.ShutdownListener(m_socket);
            }
        }

        public bool IsConnected()
        {
            return m_socket != null && SocketUtils.IsConnected(m_socket);
        }

        public void Send(string? text)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentNullException(nameof(text));

            SocketUtils.Send(m_socket, text, this);
        }

        public string Read(TimeSpan timeoutSpan)
        {
            using (CancellationTokenSource cancelSource = new CancellationTokenSource())
            {
                CancellationToken cancelToken = cancelSource.Token;
                Task<string> readTask = ReadAsync(cancelToken);

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

        public string Read()
        {
            return Read(Timeout.InfiniteTimeSpan);
        }

        public Task<string> ReadAsync(CancellationToken cancelToken = default)
        {
            return SocketUtils.ReadAsync(m_socket, this, cancelToken);
        }
    }
}
