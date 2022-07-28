using MySharpChat.Client.Command;
using MySharpChat.Client.Input;
using MySharpChat.Client.UI;
using MySharpChat.Core.Command;
using MySharpChat.Core.SocketModule;
using MySharpChat.Core.Utils;
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

        private readonly ClientNetworkModule _networkModule;
        public ClientNetworkModule NetworkModule => _networkModule;

        private readonly ClientOutputWriter _outputWriter;
        public ClientOutputWriter OutputWriter => _outputWriter;

        public string LocalEndPoint => throw new NotImplementedException();

        public string RemoteEndPoint => throw new NotImplementedException();

        public IClientLogic CurrentLogic { get; set; }

        private readonly LoaderClientLogic loaderLogic = new LoaderClientLogic();

        public DefaultClientImpl()
        {
            CurrentLogic = loaderLogic;
            _outputWriter = new ClientOutputWriter(new ConsoleOutputWriter());
            _networkModule = new ClientNetworkModule(_outputWriter);
        }

        public void Run(Client client)
        {
            // TODO reorganise to support read/write from network while reading inputs
            OutputWriter.Write(CurrentLogic.Prefix);

            IUserInputCursorHandler cursolHandler = new ConsoleCursorHandler(new ConsoleCursorContext());
            ReadingState readingState = new ReadingState(new UserInputTextHandler(), cursolHandler, OutputWriter);
            Task<string> userInputTask = CommandInput.ReadLineAsync(readingState);

            if (_networkModule.IsConnected())
            {
                while (!userInputTask.Wait(TimeSpan.FromSeconds(1)))
                {
                    string readText = _networkModule.Read(TimeSpan.FromSeconds(1));
                    if (!string.IsNullOrEmpty(readText))
                    {
                        using (OutputWriter.Lock())
                        {
                            cursolHandler.MovePositionToOrigin(CursorUpdateMode.GraphicalOnly);
                            int prefixLength = CurrentLogic.Prefix.Length;
                            cursolHandler.MovePositionNegative(prefixLength, CursorUpdateMode.GraphicalOnly);
                            int inputTextLength = cursolHandler.Position;
                            for (int i = 0; i < prefixLength + inputTextLength; i++)
                                OutputWriter.Write(" ");
                            cursolHandler.MovePositionNegative(prefixLength + inputTextLength, CursorUpdateMode.GraphicalOnly);
                            OutputWriter.WriteLine("server> {0}", readText);
                            OutputWriter.Write(CurrentLogic.Prefix);
                        }
                    }
                }
            }
            else
            {
                userInputTask.Wait();
            }

            CommandParser parser = CurrentLogic.CommandParser;
            string text = userInputTask.Result;
            if (parser.TryParse(text, out string[] args, out IClientCommand? clientCommand))
            {
                clientCommand?.Execute(this, args);
            }
            else if (parser.TryParse(text, out args, out IAsyncMachineCommand? command))
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

        public void Stop()
        {
            _networkModule.Disconnect();
            CurrentLogic = loaderLogic;
        }
    }
}
