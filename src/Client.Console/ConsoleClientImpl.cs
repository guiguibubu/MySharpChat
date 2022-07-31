using MySharpChat.Client.Command;
using MySharpChat.Client.Console;
using MySharpChat.Client.Input;
using MySharpChat.Core.Command;
using MySharpChat.Core.Packet;
using MySharpChat.Core.UI;
using MySharpChat.Core.Utils;
using MySharpChat.Core.Utils.Logger;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MySharpChat.Client
{
    internal class ConsoleClientImpl : IClientImpl
    {
        private static readonly Logger logger = Logger.Factory.GetLogger<ConsoleClientImpl>();

        private readonly INetworkModule networkModule;
        public INetworkModule NetworkModule => networkModule;

        private readonly IUserInterfaceModule userInterfaceModule;
        public IUserInterfaceModule UserInterfaceModule => userInterfaceModule;

        public string LocalEndPoint => networkModule.LocalEndPoint;

        public string RemoteEndPoint => networkModule.RemoteEndPoint;

        public IClientLogic CurrentLogic { get; set; }

        public Guid ClientId { get; private set; } = Guid.Empty;

        private readonly LoaderClientLogic loaderLogic = new LoaderClientLogic();

        private readonly CommandInput commandInput;

        public ConsoleClientImpl()
        {
            CurrentLogic = loaderLogic;
            userInterfaceModule = new ConsoleUserInterfaceModule();
            networkModule = new ClientNetworkModule(this);
            commandInput = new CommandInput(userInterfaceModule);
        }

        public void Run(Client client)
        {
            // TODO reorganise to support read/write from network while reading inputs
            IUserInputCursorHandler cursorHandler = userInterfaceModule.CursorHandler;
            LockTextWriter writer = userInterfaceModule.OutputWriter;
            writer.Write(CurrentLogic.Prefix);

            ReadingState readingState = new ReadingState(new UserInputTextHandler(), userInterfaceModule);
            Task<string> userInputTask = commandInput.ReadLineAsync(readingState);

            if (networkModule.IsConnected())
            {
                while (!userInputTask.Wait(TimeSpan.FromSeconds(1)))
                {
                    if (networkModule.HasDataAvailable)
                    {
                        List<PacketWrapper> packets = networkModule.Read(TimeSpan.FromSeconds(1));
                        foreach (PacketWrapper packet in packets)
                        {
                            if (packet.Package is ConnectionInitialisationPacket connectInitPackage)
                            {
                                ClientId = connectInitPackage.SessionId;
                            }
                            else if (packet.Package is ChatPacket chatPackage)
                            {
                                string readText = chatPackage.Message;
                                if (!string.IsNullOrEmpty(readText))
                                {
                                    using (writer.Lock())
                                    {
                                        cursorHandler.MovePositionToOrigin(CursorUpdateMode.GraphicalOnly);
                                        int prefixLength = CurrentLogic.Prefix.Length;
                                        cursorHandler.MovePositionNegative(prefixLength, CursorUpdateMode.GraphicalOnly);
                                        int inputTextLength = cursorHandler.Position;
                                        for (int i = 0; i < prefixLength + inputTextLength; i++)
                                            writer.Write(" ");
                                        cursorHandler.MovePositionNegative(prefixLength + inputTextLength, CursorUpdateMode.GraphicalOnly);
                                        writer.WriteLine("server> {0}", readText);
                                        writer.Write(CurrentLogic.Prefix);
                                    }
                                }
                            }
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
                writer.WriteLine("Fail to parse \"{0}\"", text);
                writer.WriteLine();
                writer.WriteLine("Available commands");
                parser.GetHelpCommand().Execute(writer);
            }
            writer.WriteLine();
        }

        public void Stop()
        {
            networkModule.Disconnect();
            CurrentLogic = loaderLogic;
        }
    }
}
