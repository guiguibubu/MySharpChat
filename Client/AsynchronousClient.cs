using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Diagnostics;

using MySharpChat.SocketModule;

namespace MySharpChat.Client
{
    class AsynchronousClient
    {
        // ManualResetEvent instances signal completion.  
        private readonly ManualResetEvent connectDone = new ManualResetEvent(false);
        private readonly ManualResetEvent sendDone = new ManualResetEvent(false);
        private readonly ManualResetEvent receiveDone = new ManualResetEvent(false);

        private readonly ConnexionInfos m_connexionInfos = null;
        private bool m_clientRun = false;
        private Thread m_clientThread = null;
        private Socket m_socketHandler = null;

        // The response from the remote device.  
        private string response = string.Empty;

        public AsynchronousClient(ConnexionInfos connexionInfos)
        {
            m_connexionInfos = connexionInfos;
        }

        public bool Start()
        {
            // Connect to a remote device.  

            // Establish the remote endpoint for the socket.  
            // The name of the
            // remote device is "host.contoso.com".  
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
            IPEndPoint remoteEP = SocketUtils.CreateEndPoint(m_connexionInfos);

            // Create a TCP/IP socket.  
            m_socketHandler = SocketUtils.OpenListener(m_connexionInfos);

            bool clientStarted = false;

            try
            {
                m_clientThread = new Thread(Run);
                m_clientThread.Start(new ConnectionContext() { m_remoteEP = remoteEP, m_client = this });

                while (!m_clientRun && sw.Elapsed < TimeSpan.FromSeconds(1)) { Thread.SpinWait(100); }

                Console.WriteLine("Client started (in {0} ms) !", sw.ElapsedMilliseconds);

                clientStarted = true;

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return clientStarted;
        }

        private static void Run(object connectionContextObj)
        {
            ConnectionContext context = (ConnectionContext)connectionContextObj;
            AsynchronousClient client = context.m_client;
            EndPoint remoteEP = context.m_remoteEP;

            if (Thread.CurrentThread.Name == null)
            {
                Thread.CurrentThread.Name = "RunningClientThread";
                Console.WriteLine("{0} started (Thread {1})", Thread.CurrentThread.Name, Thread.CurrentThread.ManagedThreadId);
            }

            client.m_clientRun = true;
            while (client.m_clientRun)
            {
                // Connect to the remote endpoint.  
                client.m_socketHandler.BeginConnect(remoteEP, ConnectCallback, client);
                client.connectDone.WaitOne();
                client.m_socketHandler.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

                Console.WriteLine("Connection success to {0} : {1}:{2}", client.m_connexionInfos.Hostname, client.m_connexionInfos.Ip, client.m_connexionInfos.Port);

                Console.Write(string.Format("{0}@{1}> ", Environment.UserName, client.m_socketHandler.LocalEndPoint));
                string text = Console.ReadLine();

                // Send test data to the remote device.  
                SocketUtils.Send(client.m_socketHandler, $"{text}<EOF>", SendCallback, client);
                client.sendDone.WaitOne();

                // Receive the response from the remote device.  
                client.response = SocketUtils.Read(client.m_socketHandler, ReadCallback, client);
                client.receiveDone.WaitOne();

                // Write the response to the console.  
                Console.WriteLine("Response received : {0}", client.response);

                // Release the socket.  
                client.m_socketHandler.Shutdown(SocketShutdown.Both);
                client.m_socketHandler.Close();

                client.Stop();
            }

            Console.WriteLine("Client stopped !");
        }

        public bool IsRunning()
        {
            return m_clientRun;
        }

        public void Stop()
        {
            m_clientRun = false;
        }

        public void Wait()
        {
            m_clientThread.Join();
        }

        public bool Wait(int millisecondsTimeout)
        {
            return m_clientThread.Join(millisecondsTimeout);
        }

        private static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                AsynchronousClient client = (AsynchronousClient)ar.AsyncState;
                Socket handler = client.m_socketHandler;

                // Complete the connection.  
                handler.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}", handler.RemoteEndPoint.ToString());

                // Signal that the connection has been made.  
                client.connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void ReadCallback(IAsyncResult ar)
        {
            SocketContext context = (SocketContext)ar.AsyncState;
            SocketUtils.ReadCallback(ar);

            AsynchronousClient client = (AsynchronousClient)context.owner;
            client.receiveDone.Set();
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                SocketContext context = (SocketContext)ar.AsyncState;
                int bytesSent = SocketUtils.SendCallback(ar, out string text);
                Console.WriteLine("Send {0} bytes to Server. {2}Data :{2}{1}", bytesSent, text, Environment.NewLine);

                // Signal that all bytes have been sent.  
                AsynchronousClient client = (AsynchronousClient)context.owner;
                client.sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private sealed class ConnectionContext
        {
            public EndPoint m_remoteEP;
            public AsynchronousClient m_client;
        }
    }
}
