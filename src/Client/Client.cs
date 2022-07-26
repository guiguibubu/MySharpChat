using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;

using MySharpChat.Core.Command;
using MySharpChat.Core.SocketModule;
using MySharpChat.Core.Utils;
using MySharpChat.Client.Command;
using MySharpChat.Core.Utils.Logger;
using System.Threading.Tasks;
using MySharpChat.Client.Input;
using System.IO;

namespace MySharpChat.Client
{
    public class Client : IAsyncMachine
    {
        private bool m_clientRun = false;
        private Thread? m_clientThread = null;
        private Socket? m_socketHandler = null;

        private readonly LoaderClientLogic loaderLogic = new LoaderClientLogic();

        private IClientLogic currentLogic;

        private static readonly Logger logger = Logger.Factory.GetLogger<Client>();

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

                logger.LogInfo("Client started (in {0} ms) !", sw.ElapsedMilliseconds);

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
                logger.LogDebug(string.Format("{0} started (Thread {1})", Thread.CurrentThread.Name, Thread.CurrentThread.ManagedThreadId));
            }

            m_clientRun = true;
            while (m_clientRun)
            {
                // TODO reorganise to support read/write from network while reading inputs
                ConsoleOutputWriter consoleOutputWriter = new ConsoleOutputWriter();
                consoleOutputWriter.Write(currentLogic.Prefix);

                IUserInputCursorHandler cursolHandler = new ConsoleCursorHandler(new ConsoleCursorContext());
                ReadingState readingState = new ReadingState(new UserInputTextHandler(), cursolHandler, consoleOutputWriter);
                Task<string> userInputTask = CommandInput.ReadLineAsync(readingState);

                if (IsConnected(m_socketHandler))
                {
                    while (!userInputTask.Wait(TimeSpan.FromSeconds(1)))
                    {
                        string readText = Read(TimeSpan.FromSeconds(1));
                        if (!string.IsNullOrEmpty(readText))
                        {
                            using (consoleOutputWriter.Lock())
                            {
                                cursolHandler.MovePositionToOrigin(CursorUpdateMode.GraphicalOnly);
                                int prefixLength = currentLogic.Prefix.Length;
                                cursolHandler.MovePositionNegative(prefixLength, CursorUpdateMode.GraphicalOnly);
                                int inputTextLength = cursolHandler.Position;
                                for (int i = 0; i < prefixLength + inputTextLength; i++)
                                    consoleOutputWriter.Write(" ");
                                cursolHandler.MovePositionNegative(prefixLength + inputTextLength, CursorUpdateMode.GraphicalOnly);
                                consoleOutputWriter.WriteLine("server> {0}", readText);
                                consoleOutputWriter.Write(currentLogic.Prefix);
                            }
                        }
                    }
                }
                else
                {
                    userInputTask.Wait();
                }

                CommandParser parser = currentLogic.CommandParser;
                string text = userInputTask.Result;
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

        public bool IsConnected(Socket? socket)
        {
            return socket != null && SocketUtils.IsConnected(socket);
        }

        public void Stop(int exitCode = 0)
        {
            Disconnect(m_socketHandler);
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

            const int CONNECTION_TIMEOUT_MS = 5000;

            bool timeout = ConnectImpl(this, remoteEP, out bool isConnected, CONNECTION_TIMEOUT_MS);

            if (isConnected)
            {
                Console.WriteLine("Connection success to {0} : {1}:{2}", connexionData.Hostname, connexionData.Ip, connexionData.Port);
                currentLogic = new ChatClientLogic(m_socketHandler.LocalEndPoint!);
            }
            else
            {
                if (timeout)
                    Console.WriteLine("Connection timeout ! Fail connection in {0} ms", CONNECTION_TIMEOUT_MS);
                Console.WriteLine("Connection fail to {0} : {1}:{2}", connexionData.Hostname, connexionData.Ip, connexionData.Port);
            }


            return isConnected;
        }

        private static bool ConnectImpl(Client client, IPEndPoint remoteEP, out bool isConnected, int timeoutMs = Timeout.Infinite)
        {
            bool timeout = false;
            isConnected = false;

            Socket? socket = client.m_socketHandler;

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
                    System.Text.StringBuilder loadingText = new System.Text.StringBuilder(prefix);
                    int nbDots = attempt % (nbDotsMax + 1);
                    for (int i = 0; i < nbDots; i++)
                        loadingText.Append(".");
                    for (int i = 0; i < nbDotsMax - nbDots; i++)
                        loadingText.Append(" ");

                    int oldCursorPosition = Console.CursorLeft;
                    Console.Write(loadingText);
                    Console.CursorLeft = oldCursorPosition;

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

        public void Send(string? text)
        {
            if (string.IsNullOrEmpty(text))
                throw new ArgumentNullException(nameof(text));

            SocketUtils.Send(m_socketHandler, text, this);
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

        private Task<string> ReadAsync(CancellationToken cancelToken = default)
        {
            return SocketUtils.ReadAsync(m_socketHandler, this, cancelToken);
        }

        public void Disconnect(Socket? socket)
        {
            if (socket == null)
            {
                socket = m_socketHandler;
                SocketUtils.ShutdownListener(socket);
                currentLogic = loaderLogic;
            }
        }
    }
}
