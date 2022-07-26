using MySharpChat.Client.Command;
using MySharpChat.Client.Input;
using MySharpChat.Client.UI;
using MySharpChat.Core.Command;
using MySharpChat.Core.SocketModule;
using MySharpChat.Core.Utils.Logger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MySharpChat.Client
{
    internal class DefaultClientImpl : IClientImpl
    {
        private static readonly Logger logger = Logger.Factory.GetLogger<DefaultClientImpl>();

        private readonly ClientOutputWriter _outputWriter = new ClientOutputWriter(new ConsoleOutputWriter());
        public ClientOutputWriter OutputWriter => _outputWriter;

        private readonly LoaderClientLogic loaderLogic = new LoaderClientLogic();

        private IClientLogic currentLogic;

        private Socket? m_socketHandler = null;

        public DefaultClientImpl()
        {
            currentLogic = loaderLogic;
        }

        public void Run(Client client)
        {
            // TODO reorganise to support read/write from network while reading inputs
            OutputWriter.Write(currentLogic.Prefix);

            IUserInputCursorHandler cursolHandler = new ConsoleCursorHandler(new ConsoleCursorContext());
            ReadingState readingState = new ReadingState(new UserInputTextHandler(), cursolHandler, OutputWriter);
            Task<string> userInputTask = CommandInput.ReadLineAsync(readingState);

            if (IsConnected())
            {
                while (!userInputTask.Wait(TimeSpan.FromSeconds(1)))
                {
                    string readText = Read(TimeSpan.FromSeconds(1));
                    if (!string.IsNullOrEmpty(readText))
                    {
                        using (OutputWriter.Lock())
                        {
                            cursolHandler.MovePositionToOrigin(CursorUpdateMode.GraphicalOnly);
                            int prefixLength = currentLogic.Prefix.Length;
                            cursolHandler.MovePositionNegative(prefixLength, CursorUpdateMode.GraphicalOnly);
                            int inputTextLength = cursolHandler.Position;
                            for (int i = 0; i < prefixLength + inputTextLength; i++)
                                OutputWriter.Write(" ");
                            cursolHandler.MovePositionNegative(prefixLength + inputTextLength, CursorUpdateMode.GraphicalOnly);
                            OutputWriter.WriteLine("server> {0}", readText);
                            OutputWriter.Write(currentLogic.Prefix);
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
            if (parser.TryParse(text, out string[] args, out IClientCommand? clientCommand))
            {
                clientCommand?.Execute(this, args);
            }
            else if (parser.TryParse(text, out args, out ICommand? command))
            {
                command?.Execute(client, args);
            }
            else
            {
                OutputWriter.WriteLine("Fail to parse \"{0}\"", text);
                OutputWriter.WriteLine();
                OutputWriter.WriteLine("Available commands");
                parser.GetHelpCommand().Execute();
            }
            OutputWriter.WriteLine();
        }

        public bool Connect(IPEndPoint remoteEP, out bool isConnected, int timeoutMs = Timeout.Infinite)
        {
            bool timeout = false;
            isConnected = false;

            Socket? socket = m_socketHandler;

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
                    OutputWriter.Write(loadingText);
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

        public bool Connect(ConnexionInfos connexionInfos)
        {
            ConnexionInfos.Data? connexionData = connexionInfos.Remote;
            if (connexionData == null)
                throw new ArgumentException(nameof(connexionInfos.Remote));

            IPEndPoint remoteEP = SocketUtils.CreateEndPoint(connexionData);

            // Create a TCP/IP socket.  
            m_socketHandler = SocketUtils.OpenListener(connexionData);

            const int CONNECTION_TIMEOUT_MS = 5000;

            bool timeout = Connect(remoteEP, out bool isConnected, CONNECTION_TIMEOUT_MS);

            if (isConnected)
            {
                OutputWriter.WriteLine("Connection success to {0} : {1}:{2}", connexionData.Hostname, connexionData.Ip, connexionData.Port);
                currentLogic = new ChatClientLogic(m_socketHandler.LocalEndPoint!);
            }
            else
            {
                if (timeout)
                    OutputWriter.WriteLine("Connection timeout ! Fail connection in {0} ms", CONNECTION_TIMEOUT_MS);
                OutputWriter.WriteLine("Connection fail to {0} : {1}:{2}", connexionData.Hostname, connexionData.Ip, connexionData.Port);
            }

            return isConnected;
        }

        public void Disconnect()
        {
            if (m_socketHandler != null)
            {
                SocketUtils.ShutdownListener(m_socketHandler);
                currentLogic = loaderLogic;
            }
        }

        public bool IsConnected()
        {
            return m_socketHandler != null && SocketUtils.IsConnected(m_socketHandler);
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

        public Task<string> ReadAsync(CancellationToken cancelToken = default)
        {
            return SocketUtils.ReadAsync(m_socketHandler, this, cancelToken);
        }
    }
}
