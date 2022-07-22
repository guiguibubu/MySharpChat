using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;

using MySharpChat.Core.Command;
using MySharpChat.Core.SocketModule;
using MySharpChat.Core.Utils;
using MySharpChat.Client.Command;


namespace MySharpChat.Client
{
    public class Client : IAsyncMachine
    {
        // ManualResetEvent instances signal completion.  
        private readonly ManualResetEvent connectDone = new ManualResetEvent(false);
        private readonly ManualResetEvent sendDone = new ManualResetEvent(false);
        private readonly ManualResetEvent receiveDone = new ManualResetEvent(false);

        private bool m_clientRun = false;
        private Thread? m_clientThread = null;
        private Socket? m_socketHandler = null;

        private readonly LoaderClientLogic loaderLogic = new LoaderClientLogic();

        private IClientLogic currentLogic;

        public Client()
        {
            Initialize();
        }

        ~Client()
        {
            Stop();
        }

        public void Initialize(object? initObject = null)
        {
            currentLogic = loaderLogic;
        }

        public void InitCommands()
        {
        }

        public bool Start(object? startObject = null)
        {
            return Start(startObject as string);
        }

        public bool Start(string? serverAdress)
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw.Start();

            bool clientStarted = false;

            try
            {
                m_clientThread = new Thread(Run);
                m_clientThread.Start();

                while (!m_clientRun && sw.Elapsed < TimeSpan.FromSeconds(1)) { Thread.SpinWait(100); }

                Console.WriteLine("Client started (in {0} ms) !" + Environment.NewLine, sw.ElapsedMilliseconds);

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
                Console.Write(currentLogic.Prefix);

                string? text = CommandInput.ReadLine();

                CommandParser parser = currentLogic.CommandParser;
                if (parser.TryParse(text, out string[] args, out ICommand? command))
                {
                    command?.Execute(this, args);
                }
                else
                {
                    Console.WriteLine("Fail to parse \"{0}\"", text);
                    Console.WriteLine();
                    Console.WriteLine("Available commands");
                    parser.GetHelpCommand().Execute();
                }
                Console.WriteLine();
            }

            Console.WriteLine("Client stopped !");
        }

        public bool IsRunning()
        {
            return m_clientRun;
        }

        public bool IsConnected(ConnexionInfos? connexionInfos = null)
        {
            return m_socketHandler != null && m_socketHandler.Connected;
        }

        public void Stop(int exitCode = 0)
        {
            Disconnect(null);
            m_clientRun = false;
            ExitCode = exitCode;
        }

        public void Wait()
        {
            m_clientThread?.Join();
        }

        public bool Wait(int millisecondsTimeout)
        {
            return m_clientThread?.Join(millisecondsTimeout) ?? true;
        }

        public int ExitCode { get; private set; }

        public bool Connect(ConnexionInfos connexionInfos)
        {
            ConnexionInfos.Data? connexionData = connexionInfos.Remote;
            if (connexionData == null)
                throw new ArgumentException(nameof(connexionInfos.Remote));

            IPEndPoint remoteEP = SocketUtils.CreateEndPoint(connexionData);

            // Create a TCP/IP socket.  
            m_socketHandler = SocketUtils.OpenListener(connexionData);

            // Connect to the remote endpoint.  
            IAsyncResult result = m_socketHandler.BeginConnect(remoteEP, ConnectCallback, this);

            const int TIMEOUT_MS = 5000;
            bool timeout = result.AsyncWaitHandle.WaitOne(TIMEOUT_MS, true);

            if (timeout)
                Console.WriteLine("Connection timeout ! Fail connection in {0} ms", TIMEOUT_MS);

            bool isConnected = IsConnected();
            if (isConnected)
            {
                Console.WriteLine("Connection success to {0} : {1}:{2}", connexionData.Hostname, connexionData.Ip, connexionData.Port);
                m_socketHandler.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
                currentLogic = new ChatClientLogic(m_socketHandler!.LocalEndPoint!);
            }
            else
                Console.WriteLine("Connection fail to {0} : {1}:{2}", connexionData.Hostname, connexionData.Ip, connexionData.Port);

            return isConnected;
        }

        public void Send(string? text)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentNullException(nameof(text));

            // Send test data to the remote device.  
            if (SocketUtils.Send(m_socketHandler!, $"{text}", SendCallback, this))
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
                currentLogic = loaderLogic;
            }
        }

        private static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                if (ar.AsyncState is Client client
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
                && context.owner is Client client)
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
                    && context.owner is Client client)
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
