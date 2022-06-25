using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Web;
using System.Text;
using System.Threading;

using MySharpChat;

namespace MySharpChat.Server
{
    class AsynchronousServer
    {
        // Thread signal.  
        private static readonly ManualResetEvent newThreadDone = new ManualResetEvent(false);
        private static readonly ManualResetEvent connectDone = new ManualResetEvent(false);
        private static readonly ManualResetEvent sendDone = new ManualResetEvent(false);
        private static readonly ManualResetEvent receiveDone = new ManualResetEvent(false);

        protected AsynchronousServer()
        {
        }

        public static void StartListening()
        {
            if (Thread.CurrentThread.Name == null)
            {
                Thread.CurrentThread.Name = "MainServerThread";
            }

            // Establish the local endpoint for the socket.  
            // The DNS name of the computer  
            // running the listener is "host.contoso.com".
            ConnexionInfos connexionInfos = new ConnexionInfos();
#if DEBUG
            connexionInfos.Hostname = "localhost";
#else
            connexionInfos.Hostname = Dns.GetHostName();
#endif
            IPHostEntry ipHostInfo = Dns.GetHostEntry(connexionInfos.Hostname);
            connexionInfos.Ip = ipHostInfo.AddressList.First(a => a.AddressFamily == AddressFamily.InterNetwork);
            connexionInfos.Port = ConnexionInfos.DEFAULT_PORT;
            IPEndPoint localEndPoint = SocketUtils.CreateEndPoint(connexionInfos);

            // Create a TCP/IP socket.  
            Socket listener = SocketUtils.OpenListener(connexionInfos);

            // Bind the socket to the local endpoint and listen for incoming connections.  
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                Console.WriteLine("Server started !");
                Console.WriteLine("Listenning at {0} : {1}:{2}", connexionInfos.Hostname, connexionInfos.Ip, connexionInfos.Port);

                while (true)
                {
                    // Set the event to nonsignaled state.  
                    newThreadDone.Reset();

                    // Start an asynchronous socket to listen for connections.  
                    Console.WriteLine("Waiting for a connection...");
                    listener.BeginAccept(AcceptCallback, listener);

                    // Wait until a connection is made before continuing.  
                    newThreadDone.WaitOne();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();

        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.  
            newThreadDone.Set();

            // Get the socket that handles the client request.  
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            if (Thread.CurrentThread.Name == null)
            {
                Thread.CurrentThread.Name = $"WorkingThread{handler.RemoteEndPoint}";
            }

            string content = SocketUtils.Read(handler, ReadCallback);

            //if (content.Contains("<EOF>"))
            //|| content.Contains(Environment.NewLine + Environment.NewLine))
#if DEBUG
            // All the data has been read from the
            // client. Display it on the console.  
            bool noData = content.Length == 0;
            if (!noData)
                Console.WriteLine("Read {0} bytes from socket. {2}Data :{2}{1}", content.Length, content, Environment.NewLine);
#endif

            // Echo the data back to the client.
            if (HttpParser.TryParseHttpRequest(content, out _))
            {
                string text = "Welcome on MySharpChat server.";
                content = string.Format("HTTP/1.1 {0} {1}{4}Content-Type : text/plain{4}Content-Length: {2}{4}{4}{3}{4}", (int)HttpStatusCode.OK, HttpStatusCode.OK.ToString(), text.Length, text, Environment.NewLine);
            }
            else if(content.Length > 0)
            {
                content = "No Data";
            }

            SocketUtils.Send(handler, content, SendCallback);

            // Release the socket.  
            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
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
                SocketContext state = (SocketContext)ar.AsyncState;
                Socket handler = state.workSocket;
                Console.WriteLine("Send {0} bytes to client {2}. {3}Data :{3}{1}", bytesSent, text, handler.RemoteEndPoint, Environment.NewLine);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
