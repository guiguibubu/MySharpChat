using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Web;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

using MySharpChat.Core.SocketModule;
using MySharpChat.Core.Http;
using MySharpChat.Core.Utils;

namespace MySharpChat.Server
{
    class AsynchronousServer : IAsyncMachine
    {
        // Thread signal.  
        private readonly ManualResetEvent newConnectionAvailableEvent = new ManualResetEvent(false);
        private readonly ManualResetEvent connectDone = new ManualResetEvent(false);
        private readonly ManualResetEvent sendDone = new ManualResetEvent(false);
        private readonly ManualResetEvent receiveDone = new ManualResetEvent(false);

        private readonly ConnexionInfos? m_connexionInfos = null;
        private bool m_serverRun = false;
        private Thread? m_serverThread = null;
        private Socket? m_listener = null;

        public AsynchronousServer(ConnexionInfos connexionInfos)
        {
            m_connexionInfos = connexionInfos;
            Initialize();
        }

        public void Initialize(object? initObject = null)
        {
            InitCommands();
        }

        public void InitCommands()
        {
            //No commands for the server for the moment
        }

        public bool Start(object? startObject = null)
        {
            if (Thread.CurrentThread.Name == null)
            {
                Thread.CurrentThread.Name = "MainServerThread";
            }
            // Establish the local endpoint for the socket.  
            // The DNS name of the computer  
            // running the listener is "host.contoso.com".
            if (m_connexionInfos == null)
                return false;

#if DEBUG
            m_connexionInfos.Hostname = "localhost";
#else
            m_connexionInfos.Hostname = Dns.GetHostName();
#endif
            Stopwatch sw = Stopwatch.StartNew();
            sw.Start();

            IPHostEntry ipHostInfo = Dns.GetHostEntry(m_connexionInfos.Hostname);
            m_connexionInfos.Ip = ipHostInfo.AddressList.First(a => a.AddressFamily == AddressFamily.InterNetwork);
            m_connexionInfos.Port = ConnexionInfos.DEFAULT_PORT;
            IPEndPoint localEndPoint = SocketUtils.CreateEndPoint(m_connexionInfos);

            // Create a TCP/IP socket.  
            m_listener = SocketUtils.OpenListener(m_connexionInfos);

            bool serverStarted = false;
            // Bind the socket to the local endpoint and listen for incoming connections.  
            try
            {
                m_listener.Bind(localEndPoint);
                m_listener.Listen(100);

                m_serverThread = new Thread(Run);
                m_serverThread.Start(this);

                while (!m_serverRun && sw.Elapsed < TimeSpan.FromSeconds(1)) { Thread.SpinWait(100); }

                Console.WriteLine("Server started (in {0} ms) !", sw.ElapsedMilliseconds);
                Console.WriteLine("Listenning at {0} : {1}:{2}", m_connexionInfos.Hostname, m_connexionInfos.Ip, m_connexionInfos.Port);

                serverStarted = true;

            }
            catch (Exception e)
            {
                Console.WriteLine("Fail to start server");
                Console.WriteLine(e.ToString());
            }

            return serverStarted;
        }

        public bool IsRunning()
        {
            return m_serverRun;
        }

        public void Stop()
        {
            m_serverRun = false;
        }

        public void Wait()
        {
            m_serverThread?.Join();
        }

        public bool Wait(int millisecondsTimeout)
        {
            return m_serverThread?.Join(millisecondsTimeout) ?? true;
        }

        private static void Run(object? serverObj)
        {
            if(serverObj == null)
                throw new ArgumentNullException(nameof(serverObj));

            AsynchronousServer server = (AsynchronousServer)serverObj;

            if (Thread.CurrentThread.Name == null)
            {
                Thread.CurrentThread.Name = "RunningServerThread";
                Console.WriteLine("{0} started (Thread {1})", Thread.CurrentThread.Name, Thread.CurrentThread.ManagedThreadId);
            }

            server.m_serverRun = true;
            while (server.m_serverRun)
            {
                // Set the event to nonsignaled state.  
                server.newConnectionAvailableEvent.Reset();

                // Start an asynchronous socket to listen for connections.  
                Console.WriteLine("Waiting for a connection...");
                server.m_listener?.BeginAccept(AcceptCallback, server);

                // Wait until a connection is made before continuing.  
                while (server.m_serverRun)
                {
                    server.newConnectionAvailableEvent.WaitOne(1000);
                }
            }

            Console.WriteLine("Server stopped !");
        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            if (ar.AsyncState is AsynchronousServer server
                && server.m_listener != null)
            {
                // Signal the main thread to continue.  
                server.newConnectionAvailableEvent.Set();

                // Get the socket that handles the client request.  
                server.m_listener = server.m_listener.EndAccept(ar);

                Console.WriteLine("Connection accepted from {0}", server.m_listener.RemoteEndPoint);

                server.m_listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

                if (Thread.CurrentThread.Name == null)
                {
                    Thread.CurrentThread.Name = $"WorkingThread{server.m_listener.RemoteEndPoint}";
                }

                string content = "";
                do
                {
                    content = SocketUtils.Read(server.m_listener, ReadCallback, server, server.receiveDone);
                } while (string.IsNullOrEmpty(content));



                // All the data has been read from the
                // client. Display it on the console.  
#if DEBUG
                Console.WriteLine("Read {0} bytes from socket. {2}Data :{2}{1}", content.Length, content, Environment.NewLine);
#endif

                // Echo the data back to the client.
                if (HttpParser.TryParseHttpRequest(content, out HttpRequestMessage? httpRequestMessage))
                {
                    string text = "Welcome on MySharpChat server.";
#pragma warning disable CS8602 // Déréférencement d'une éventuelle référence null.
                    if (!string.Equals(httpRequestMessage.RequestUri, "/"))
                    {
                        text += Environment.NewLine;
                        text += $"No data at {httpRequestMessage.RequestUri}";
                    }
#pragma warning restore CS8602 // Déréférencement d'une éventuelle référence null.
                    HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.OK);
                    response.Content = new StringContent(text);
                    content = HttpParser.ToString(response).Result;
                }

                SocketUtils.Send(server.m_listener, content, SendCallback, server);

                // Release the socket.  
                server.m_listener.Shutdown(SocketShutdown.Both);
                server.m_listener.Close();
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
                if(ar.AsyncState is SocketContext state
                    && state.workSocket != null)
                {
                    Socket handler = state.workSocket;
#if DEBUG
                    Console.WriteLine("Send {0} bytes to client {2}. {3}Data :{3}{1}", bytesSent, text, handler.RemoteEndPoint, Environment.NewLine);
#endif
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
