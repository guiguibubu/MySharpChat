using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;

using MySharpChat.Core.Command;
using MySharpChat.Core.SocketModule;
using MySharpChat.Core.Http;
using MySharpChat.Core.Utils;

using MySharpChat.Server.Command;
using MySharpChat.Core.Utils.Logger;
using System.Threading.Tasks;

namespace MySharpChat.Server
{
    public class Server : IAsyncMachine
    {
        private readonly ConnexionInfos? m_connexionInfos = null;
        private bool m_serverRun = false;
        private Thread? m_serverThread = null;
        private Socket? m_listeningSocketHandler = null;

        private readonly CommandManager commandManager = new CommandManager();

        private static readonly Logger logger = Logger.Factory.GetLogger<Server>();

        public Server(ConnexionInfos connexionInfos)
        {
            m_connexionInfos = connexionInfos;
            OutputWriter = new ServerOutputWriter(new ConsoleOutputWriter());
            Initialize();
        }

        ~Server()
        {
            Stop();
        }

        public void Initialize(object? initObject = null)
        {
            InitCommands();
        }

        public void InitCommands()
        {
            commandManager.AddCommand(ConnectCommand.Instance);
        }

        public bool Start(object? startObject = null)
        {
            if (Thread.CurrentThread.Name == null)
            {
                Thread.CurrentThread.Name = "MainServerThread";
            }

            if (m_connexionInfos == null)
                return false;

            Stopwatch sw = Stopwatch.StartNew();
            sw.Start();

            bool serverStarted = false;
            try
            {
                m_serverThread = new Thread(Run);
                m_serverThread.Start();

                while (!m_serverRun && sw.Elapsed < TimeSpan.FromSeconds(1)) { Thread.SpinWait(100); }

                logger.LogInfo("Server started (in {0} ms) !", sw.ElapsedMilliseconds);

                serverStarted = true;

            }
            catch (Exception e)
            {
                logger.LogError("Fail to start server");
                logger.LogError(e.ToString());
            }

            return serverStarted;
        }

        public bool IsRunning()
        {
            return m_serverRun;
        }

        public bool IsConnected(Socket? socket)
        {
            return socket != null && SocketUtils.IsConnected(socket);
        }

        public void Stop(int exitCode = 0)
        {
            Disconnect(null);
            m_serverRun = false;
            ExitCode = exitCode;
        }

        public void Wait()
        {
            m_serverThread?.Join();
        }

        public bool Wait(int millisecondsTimeout)
        {
            return m_serverThread?.Join(millisecondsTimeout) ?? true;
        }

        public int ExitCode { get; private set; }

        public LockTextWriter OutputWriter { get; private set; }

        private void Run()
        {
            if (Thread.CurrentThread.Name == null)
            {
                Thread.CurrentThread.Name = "RunningServerThread";
                logger.LogDebug(string.Format("{0} started (Thread {1})", Thread.CurrentThread.Name, Thread.CurrentThread.ManagedThreadId));
            }

            ConnectCommand command = commandManager.GetCommand<ConnectCommand>(ConnectCommand.Instance!.Name)!;
            command.Execute(this);

            m_serverRun = true;
            while (m_serverRun)
            {
                // Start an asynchronous socket to listen for connections.  
                logger.LogDebug("Waiting for a connection...");

                Socket connectedSocket = m_listeningSocketHandler!.Accept();

                EndPoint remoteEP = connectedSocket.RemoteEndPoint!;
                logger.LogInfo("Connection accepted. Begin session with {0}", remoteEP);

                if (Thread.CurrentThread.Name == null)
                {
                    Thread.CurrentThread.Name = $"WorkingThread{remoteEP}";
                }

                RunSession(connectedSocket);

                logger.LogInfo("Connection lost. Session with {0} finished", remoteEP);

                Disconnect(connectedSocket);
            }

            logger.LogInfo("Server stopped !");
        }

        public bool Connect(ConnexionInfos connexionInfos)
        {
            ConnexionInfos.Data? connexionData = connexionInfos.Local;
            if (connexionData == null)
                throw new ArgumentException(nameof(connexionInfos.Local));

            IPEndPoint localEndPoint = SocketUtils.CreateEndPoint(connexionData);

            // Create a TCP/IP socket.  
            m_listeningSocketHandler = SocketUtils.OpenListener(connexionData);

            // Bind the socket to the local endpoint and listen for incoming connections. 
            m_listeningSocketHandler.Bind(localEndPoint);

            m_listeningSocketHandler.Listen(100);

            logger.LogInfo(string.Format("Listenning at {0} : {1}:{2}", connexionData.Hostname, connexionData.Ip, connexionData.Port));

            return true;
        }

        public bool Send(Socket? socket, string? text)
        {
            if (socket == null)
                throw new ArgumentNullException(nameof(socket));

            if (string.IsNullOrEmpty(text))
                throw new ArgumentNullException(nameof(text));

            return SocketUtils.Send(socket, text, this);
        }

        public string Read(Socket? socket, TimeSpan timeoutSpan)
        {
            if (socket == null)
                throw new ArgumentNullException(nameof(socket));

            using (CancellationTokenSource cancelSource = new CancellationTokenSource())
            {
                CancellationToken cancelToken = cancelSource.Token;
                Task<string> readTask = SocketUtils.ReadAsync(socket, this, cancelToken);

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
                        return readTask.Result;
                    }
                    catch (AggregateException e)
                    {
                        logger.LogError(e, "Fail to read from {0}", socket.RemoteEndPoint);
                        return string.Empty;
                    }
                }
                else
                {
                    cancelSource.Cancel();
                    logger.LogDebug("Reading timeout reached. Nothing received from {0} after {1} ms", socket.RemoteEndPoint, timeoutSpan);
                    return string.Empty;
                }
            }
        }

        public void Disconnect(Socket? socket)
        {
            if (socket != null)
            {
                SocketUtils.ShutdownListener(socket);
            }
        }

        private void RunSession(Socket? socket)
        {
            if (socket == null)
                throw new ArgumentNullException(nameof(socket));

            while (IsConnected(socket))
            {
                string content = Read(socket, TimeSpan.FromSeconds(1));

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
                    Send(socket, content);
                }
            }
        }
    }
}
