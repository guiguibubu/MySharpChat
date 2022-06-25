using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Web;
using System.Text;
using System.Threading;

namespace MySharpChat.Server
{
    // State object for reading client data asynchronously  
    public class StateObject
    {
        // Client  socket.  
        public Socket workSocket = null;
        // Size of receive buffer.  
        public const int BufferSize = 256;
        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];
        // Received data string.  
        public StringBuilder sb = new StringBuilder();
    }

    class AsynchronousServer
    {
        // Thread signal.  
        public static readonly ManualResetEvent allDone = new ManualResetEvent(false);

        protected AsynchronousServer()
        {
        }

        public static void StartListening()
        {
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
            IPEndPoint localEndPoint = CreateEndPoint(connexionInfos);

            // Create a TCP/IP socket.  
            Socket listener = OpenListener(connexionInfos);

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
                    allDone.Reset();

                    // Start an asynchronous socket to listen for connections.  
                    Console.WriteLine("Waiting for a connection...");
                    listener.BeginAccept(
                        new AsyncCallback(AcceptCallback),
                        listener);

                    // Wait until a connection is made before continuing.  
                    allDone.WaitOne();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();

        }

        public static Socket OpenListener(ConnexionInfos connexionInfos)
        {
            // Create a TCP/IP socket.  
            Socket listener = new Socket(connexionInfos.Ip.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);
            return listener;
        }

        public static IPEndPoint CreateEndPoint(ConnexionInfos connexionInfos)
        {
            IPEndPoint endPoint = new IPEndPoint(connexionInfos.Ip, connexionInfos.Port);
            return endPoint;
        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.  
            allDone.Set();

            // Get the socket that handles the client request.  
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            string content = Read(handler);

            //if (content.Contains("<EOF>"))
            //|| content.Contains(Environment.NewLine + Environment.NewLine))
            // All the data has been read from the
            // client. Display it on the console.  
            bool noData = content.Length == 0;
            if (!noData)
                Console.WriteLine("Read {0} bytes from socket. {2}Data :{2}{1}", content.Length, content, Environment.NewLine);

            /*
             GET / HTTP/1.1
            User-Agent: PostmanRuntime/7.28.4
            Accept: * /*
            Postman - Token: 48442a24 - 780f - 4664 - a9c6 - 6830ce575132
            Host: localhost: 11000
            Accept - Encoding: gzip, deflate, br
            Connection: keep - alive
            */
            bool isHttpRequest = content.Contains(" HTTP/1.1");

            //https://gist.github.com/define-private-public/d05bc52dd0bed1c4699d49e2737e80e7
            //HttpMethod

            // Echo the data back to the client.
            if (isHttpRequest)
            {
                string text = "Welcome on MySharpChat server.";
                content = string.Format("HTTP/1.1 {0} {1}\r\nContent-Type : text/plain\r\nContent-Length: {2}\r\n\r\n{3}\r\n", (int)HttpStatusCode.OK, HttpStatusCode.OK.ToString(), text.Length, text);
            }
            else if(content.Length > 0)
            {
                content = "No Data";
            }

            Send(handler, content);
        }

        public static string Read(Socket handler)
        {
            string content = string.Empty;

            // Create the state object.  
            StateObject stateReader = new StateObject();
            stateReader.workSocket = handler;
            handler.BeginReceive(stateReader.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), stateReader);

            // Check for end-of-file tag. If it is not there, read
            // more data.  
            content = stateReader.sb.ToString();

            return content;
        }

        public static void ReadCallback(IAsyncResult ar)
        {
            // Retrieve the state object and the handler socket  
            // from the asynchronous state object.  
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            if (handler.Connected)
            {
                // Read data from the client socket.
                int bytesRead = handler.EndReceive(ar);

                if (bytesRead > 0)
                {
                    // There  might be more data, so store the data received so far.  
                    state.sb.Append(Encoding.ASCII.GetString(
                        state.buffer, 0, bytesRead));

                    bool continueReceive = bytesRead == StateObject.BufferSize;
                    if (continueReceive)
                    {
                        // Not all data received. Get more.  
                        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, SocketFlags.None,
                        new AsyncCallback(ReadCallback), state);
                    }
                }
            }
        }

        private static void Send(Socket handler, string data)
        {
            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Create the state object.  
            StateObject state = new StateObject();
            state.workSocket = handler;
            state.sb.Append(data);

            // Begin sending the data to the remote device.  
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), state);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                StateObject state = (StateObject)ar.AsyncState;
                Socket handler = state.workSocket;

                // Complete sending the data to the remote device.  
                int bytesSent = handler.EndSend(ar);

                string text = state.sb.ToString();
                Console.WriteLine("Send {0} bytes to client. {2}Data :{2}{1}", bytesSent, text, Environment.NewLine);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close(1);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
