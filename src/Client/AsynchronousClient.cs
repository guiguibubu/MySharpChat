using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

using MySharpChat.Core.Command;
using MySharpChat.Core.SocketModule;
using MySharpChat.Core.Utils;
using MySharpChat.Client.Command;


namespace MySharpChat.Client
{
    public class AsynchronousClient : IAsyncMachine
    {
        // ManualResetEvent instances signal completion.  
        private readonly ManualResetEvent connectDone = new ManualResetEvent(false);
        private readonly ManualResetEvent sendDone = new ManualResetEvent(false);
        private readonly ManualResetEvent receiveDone = new ManualResetEvent(false);

        private bool m_clientRun = false;
        private Thread? m_clientThread = null;
        private Socket? m_socketHandler = null;

        public AsynchronousClient()
        {
            Initialize();
        }

        ~AsynchronousClient()
        {
            Stop();
        }

        public void Initialize(object? initObject = null)
        {
            InitCommands();
        }

        public void InitCommands()
        {
            CommandManager? commandManager = CommandManager.Instance;
            if (commandManager != null)
            {
                commandManager.AddCommand(QuitCommand.Instance);
                commandManager.AddCommand(ConnectCommand.Instance);
                commandManager.AddCommand(DisconnectCommand.Instance);
                commandManager.AddCommand(SendCommand.Instance);
                commandManager.AddCommand(HelpCommand.Instance);
            }
        }

        public bool Start(object? startObject = null)
        {
            return Start(startObject as string);
        }

        public bool Start(string? serverAdress)
        {
            // Connect to a remote device.  

            // Establish the remote endpoint for the socket.  
            // The name of the
            // remote device is "host.contoso.com".

            Stopwatch sw = Stopwatch.StartNew();
            sw.Start();

            bool clientStarted = false;

            try
            {
                m_clientThread = new Thread(Run);
                m_clientThread.Start();

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

        private void Run()
        {
            if (Thread.CurrentThread.Name == null)
            {
                Thread.CurrentThread.Name = "RunningClientThread";
                Console.WriteLine("{0} started (Thread {1})", Thread.CurrentThread.Name, Thread.CurrentThread.ManagedThreadId);
            }

            m_clientRun = true;
            while (m_clientRun)
            {
                if (IsConnected(null))
                    Console.Write(string.Format("{0}@{1}> ", Environment.UserName, m_socketHandler!.LocalEndPoint));
                else
                    Console.Write(string.Format("{0}> ", Environment.UserName));

                string? text = Console.ReadLine();

                CommandParser? parser = CommandParser.Instance;
                if (parser?.TryParse(text, out string[] args, out ICommand? command) ?? false)
                {
                    command?.Execute(this, args);
                }
                else
                {
                    Console.WriteLine("Fail to parse \"{0}\"", text);
                }
            }

            Console.WriteLine("Client stopped !");
        }

        public bool IsRunning()
        {
            return m_clientRun;
        }

        public bool IsConnected(ConnexionInfos? connexionInfos)
        {
            return m_socketHandler != null && m_socketHandler.Connected;
        }

        public void Stop()
        {
            Disconnect(null);
            m_clientRun = false;
        }

        public void Wait()
        {
            m_clientThread?.Join();
        }

        public bool Wait(int millisecondsTimeout)
        {
            return m_clientThread?.Join(millisecondsTimeout) ?? true;
        }

        public bool Connect(ConnexionInfos connexionInfos)
        {
            ConnexionInfos.Data? connexionData = connexionInfos.Remote;
            if (connexionData == null)
                throw new ArgumentException(nameof(connexionInfos.Remote));

            IPEndPoint remoteEP = SocketUtils.CreateEndPoint(connexionData);

            // Create a TCP/IP socket.  
            m_socketHandler = SocketUtils.OpenListener(connexionData);

            // Connect to the remote endpoint.  
            m_socketHandler.BeginConnect(remoteEP, ConnectCallback, this);
            connectDone.WaitOne();
            connectDone.Reset();
            m_socketHandler.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

            bool isConnected = m_socketHandler.Connected;
            if (IsConnected(null))
                Console.WriteLine("Connection success to {0} : {1}:{2}", connexionData.Hostname, connexionData.Ip, connexionData.Port);
            else
                Console.WriteLine("Connection fail to {0} : {1}:{2}", connexionData.Hostname, connexionData.Ip, connexionData.Port);

            return isConnected;
        }

        public void Send(string? text)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentNullException(nameof(text));

            // Send test data to the remote device.  
            if(SocketUtils.Send(m_socketHandler!, $"{text}", SendCallback, this))
            {
                sendDone.WaitOne();
                // Set the event to nonsignaled state.  
                sendDone.Reset();
            }
        }

        public string Read()
        {
            string result;
            // Receive the response from the remote device.  
            result = SocketUtils.Read(m_socketHandler!, ReadCallback, this, receiveDone);
            Console.WriteLine("Response received : {0}", result);
            return result;
        }

        public void Disconnect(ConnexionInfos? connexionInfos)
        {
            if (m_socketHandler != null)
            {
                SocketUtils.ShutdownListener(m_socketHandler);
            }
        }

        private static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                if (ar.AsyncState is AsynchronousClient client
                    && client.m_socketHandler != null
                    && client.m_socketHandler.RemoteEndPoint != null)
                {
                    Socket handler = client.m_socketHandler;

                    // Complete the connection.  
                    handler.EndConnect(ar);

                    Console.WriteLine("Socket connected to {0}", handler.RemoteEndPoint.ToString());

                    // Signal that the connection has been made.  
                    client.connectDone.Set();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void ReadCallback(IAsyncResult ar)
        {
            if (ar.AsyncState is SocketContext context
                && context.owner is AsynchronousClient client)
            {
                SocketUtils.ReadCallback(ar);

                client.receiveDone.Set();
            }
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                if (ar.AsyncState is SocketContext context
                    && context.owner is AsynchronousClient client)
                {
                    int bytesSent = SocketUtils.SendCallback(ar, out string text);
                    Console.WriteLine("Send {0} bytes to Server. {2}Data :{2}{1}", bytesSent, text, Environment.NewLine);

                    // Signal that all bytes have been sent.  
                    client.sendDone.Set();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
