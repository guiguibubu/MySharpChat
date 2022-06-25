using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;

namespace MySharpChat.Client
{
    class AsynchronousClient
    {
        // ManualResetEvent instances signal completion.  
        private static ManualResetEvent connectDone = new ManualResetEvent(false);
        private static ManualResetEvent sendDone = new ManualResetEvent(false);
        private static ManualResetEvent receiveDone = new ManualResetEvent(false);

        // The response from the remote device.  
        private static String response = String.Empty;

        public static void StartClient()
        {
            // Connect to a remote device.  
            try
            {
                // Establish the remote endpoint for the socket.  
                // The name of the
                // remote device is "host.contoso.com".  
                ConnexionInfos connexionInfos = new ConnexionInfos();
#if DEBUG
                connexionInfos.Hostname = "localhost";
#else
                connexionInfos.Hostname = Dns.GetHostName();
#endif
                IPHostEntry ipHostInfo = Dns.GetHostEntry(connexionInfos.Hostname);
                connexionInfos.Ip = ipHostInfo.AddressList.First(a => a.AddressFamily == AddressFamily.InterNetwork);
                connexionInfos.Port = ConnexionInfos.DEFAULT_PORT;
                IPEndPoint remoteEP = SocketUtils.CreateEndPoint(connexionInfos);

                // Create a TCP/IP socket.  
                Socket handler = SocketUtils.OpenListener(connexionInfos);

                // Connect to the remote endpoint.  
                handler.BeginConnect(remoteEP,
                    new AsyncCallback(ConnectCallback), handler);
                connectDone.WaitOne();

                // Send test data to the remote device.  
                SocketUtils.Send(handler, "This is a test<EOF>", SendCallback);
                sendDone.WaitOne();

                // Receive the response from the remote device.  
                response = SocketUtils.Read(handler, ReadCallback);
                receiveDone.WaitOne();

                // Write the response to the console.  
                Console.WriteLine("Response received : {0}", response);

                // Release the socket.  
                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();
        }

        private static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket handler = (Socket)ar.AsyncState;

                // Complete the connection.  
                handler.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}", handler.RemoteEndPoint.ToString());

                // Signal that the connection has been made.  
                connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void ReadCallback(IAsyncResult ar)
        {
            SocketUtils.ReadCallback(ar);
            receiveDone.Set();
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                int bytesSent = SocketUtils.SendCallback(ar, out string text);
                Console.WriteLine("Send {0} bytes to Server. {2}Data :{2}{1}", bytesSent, text, Environment.NewLine);

                // Signal that all bytes have been sent.  
                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
