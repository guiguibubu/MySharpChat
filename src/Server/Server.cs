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

namespace MySharpChat.Server
{
    public class Server : IAsyncMachine
    {
        // Thread signal.  
        private readonly ManualResetEvent newConnectionAvailableEvent = new ManualResetEvent(false);
        private readonly ManualResetEvent connectDone = new ManualResetEvent(false);
        private readonly ManualResetEvent sendDone = new ManualResetEvent(false);
        private readonly ManualResetEvent receiveDone = new ManualResetEvent(false);

        private readonly ConnexionInfos? m_connexionInfos = null;
        private bool m_serverRun = false;
        private Thread? m_serverThread = null;
        private Socket? m_socketHandler = null;

        private readonly CommandManager commandManager = new CommandManager();

        private static readonly Logger logger = Logger.Factory.GetLogger<Server>();

        public Server(ConnexionInfos connexionInfos)
        {
            m_connexionInfos = connexionInfos;
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

                logger.LogInfo(string.Format("Server started (in {0} ms) !", sw.ElapsedMilliseconds));

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

        public bool IsConnected(ConnexionInfos? connexionInfos = null)
        {
            return m_socketHandler != null && SocketUtils.IsConnected(m_socketHandler);
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
                // Set the event to nonsignaled state.  
                newConnectionAvailableEvent.Reset();

                // Start an asynchronous socket to listen for connections.  
                logger.LogDebug("Waiting for a connection...");
                m_socketHandler?.BeginAccept(AcceptCallback, this);

                // Wait until a connection is made before continuing.  
                while (m_serverRun)
                {
                    newConnectionAvailableEvent.WaitOne(1000);
                }
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
            m_socketHandler = SocketUtils.OpenListener(connexionData);

            // Bind the socket to the local endpoint and listen for incoming connections. 
            m_socketHandler.Bind(localEndPoint);
            m_socketHandler.Listen(100);

            logger.LogInfo(string.Format("Listenning at {0} : {1}:{2}", connexionData.Hostname, connexionData.Ip, connexionData.Port));

            return true;
        }

        public void Send(string? text)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentNullException(nameof(text));

            SocketUtils.Send(m_socketHandler!, text, SendCallback, this);
        }

        public string Read()
        {
            return SocketUtils.Read(m_socketHandler!, ReadCallback, this, receiveDone);
        }

        public void Disconnect(ConnexionInfos? connexionInfos)
        {
            if (m_socketHandler != null)
            {
                SocketUtils.ShutdownListener(m_socketHandler);
            }
        }

        private void RunSession()
        {
            while (IsConnected())
            {
                // TODO Fix the read server side !!!!!!!!!!!!!
                string content = "";
                do
                {
                    content = Read();
                } while (string.IsNullOrEmpty(content));

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
                Send(content);
            }
        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            if (ar.AsyncState is Server server
                && server.m_socketHandler != null)
            {
                // Signal the main thread to continue.  
                server.newConnectionAvailableEvent.Set();

                // Get the socket that handles the client request.  
                server.m_socketHandler = server.m_socketHandler.EndAccept(ar);

                // TODO Better handle session life circle

                EndPoint remoteEP = server.m_socketHandler.RemoteEndPoint!;
                logger.LogInfo(string.Format("Connection accepted. Begin session with {0}", remoteEP));

                server.m_socketHandler.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

                if (Thread.CurrentThread.Name == null)
                {
                    Thread.CurrentThread.Name = $"WorkingThread{remoteEP}";
                }

                server.RunSession();

                logger.LogInfo(string.Format("Session with {0} finished", remoteEP));
            }
        }

        public static void ReadCallback(IAsyncResult ar)
        {
            SocketUtils.ReadCallback(ar);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                int bytesSent = SocketUtils.SendCallback(ar, out string text);
                if (ar.AsyncState is SocketContext state
                    && state.workSocket != null)
                {
                    Socket handler = state.workSocket;
                    logger.LogDebug(string.Format("Send {0} bytes to client {2}. Data :{1}", bytesSent, text, handler.RemoteEndPoint));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
