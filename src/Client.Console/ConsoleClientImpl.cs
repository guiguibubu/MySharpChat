using MySharpChat.Client.Command;
using MySharpChat.Client.Console.Command;
using MySharpChat.Client.Input;
using MySharpChat.Core.Command;
using MySharpChat.Core.Packet;
using MySharpChat.Core.UI;
using MySharpChat.Core.Utils;
using MySharpChat.Core.Utils.Logger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MySharpChat.Client.Console
{
    internal class ConsoleClientImpl : BaseClientImpl
    {
        private static readonly Logger logger = Logger.Factory.GetLogger<ConsoleClientImpl>();

        private readonly IUserInterfaceModule m_userInterfaceModule;
        public IUserInterfaceModule UserInterfaceModule => m_userInterfaceModule;

        public IClientLogic CurrentLogic { get; set; }
        protected readonly LoaderClientLogic loaderLogic;

        protected readonly CommandInput commandInput;

        public ConsoleClientImpl() : base()
        {
            m_userInterfaceModule = new ConsoleUserInterfaceModule();
            commandInput = new CommandInput(m_userInterfaceModule);
            loaderLogic = new LoaderClientLogic(this);
            CurrentLogic = loaderLogic;
        }

        public override void Run(Client client)
        {
            // TODO reorganise to support read/write from network while reading inputs
            LockTextWriter writer = m_userInterfaceModule.OutputWriter;
            string currentPrefix = CurrentLogic.Prefix;
            writer.Write(currentPrefix);

            ReadingState readingState = new ReadingState(new UserInputTextHandler(), m_userInterfaceModule);
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
                            if (packet.Package is ClientInitialisationPacket connectInitPackage)
                            {
                                bool isInitialised = ClientId != Guid.Empty;
                                if (isInitialised)
                                {
                                    string newUsername = connectInitPackage.Username;
                                    if (!string.IsNullOrEmpty(newUsername))
                                        Username = newUsername;
                                }
                                else
                                {
                                    ClientId = connectInitPackage.SessionId;
                                    // Tell the server our username
                                    ClientInitialisationPacket initPacket = new ClientInitialisationPacket(ClientId, Username);
                                    PacketWrapper packetWrapper = new PacketWrapper(ClientId, initPacket);
                                    NetworkModule.Send(packetWrapper);
                                }

                            }
                            else if (packet.Package is ChatPacket chatPackage)
                            {
                                HandleChatPacket(chatPackage);
                            }
                        }
                    }

                    Stopwatch stopwatch = Stopwatch.StartNew();
                    long graphicalTimeoutMs = (long)TimeSpan.FromSeconds(2).TotalMilliseconds;

                    bool graphicalUpdateTimeout = stopwatch.ElapsedMilliseconds > graphicalTimeoutMs;
                    if (graphicalUpdateTimeout)
                    {
                        string previousInputNotValidated = readingState.InputTextHandler.ToString();
                        IUserInputCursorHandler cursorHandler = m_userInterfaceModule.CursorHandler;
                        using (writer.Lock())
                        {
                            cursorHandler.MovePositionToOrigin(CursorUpdateMode.GraphicalOnly);
                            int prefixLength = currentPrefix.Length;
                            cursorHandler.MovePositionNegative(prefixLength, CursorUpdateMode.GraphicalOnly);
                            int inputTextLength = cursorHandler.Position;
                            for (int i = 0; i < prefixLength + inputTextLength; i++)
                                writer.Write(" ");
                            cursorHandler.MovePositionNegative(prefixLength + inputTextLength, CursorUpdateMode.GraphicalOnly);
                            currentPrefix = CurrentLogic.Prefix;
                            writer.Write(currentPrefix);
                            writer.Write(previousInputNotValidated);
                        }
                        stopwatch.Restart();
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
                if (!clientCommand!.Execute(this, args))
                {
                    writer.WriteLine("Fail of command \"{0}\"", text);
                    HelpCommand helpCommand = parser.GetHelpCommand();
                    helpCommand.Execute(writer, clientCommand.Name);
                }
            }
            else if (parser.TryParse(text, out args, out IAsyncMachineCommand? asyncMachineCommand))
            {
                if (!asyncMachineCommand!.Execute(client, args))
                {
                    writer.WriteLine("Fail of command \"{0}\"", text);
                    HelpCommand helpCommand = parser.GetHelpCommand();
                    helpCommand.Execute(writer, asyncMachineCommand.Name);
                }
            }
            else if (parser.TryParse(text, out args, out ConsoleCommand? consoleCommand))
            {
                object? data = null;
                if (consoleCommand!.UnderlyingCommandIs(typeof(IClientCommand)))
                {
                    data = this;
                }
                else if (consoleCommand!.UnderlyingCommandIs(typeof(IAsyncMachineCommand)))
                {
                    data = client;
                }
                if (!consoleCommand!.Execute(data, args))
                {
                    writer.WriteLine("Fail of command \"{0}\"", text);
                    HelpCommand helpCommand = parser.GetHelpCommand();
                    helpCommand.Execute(writer, consoleCommand.Name);
                }
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

        private void HandleChatPacket(ChatPacket chatPacket)
        {
            IUserInputCursorHandler cursorHandler = m_userInterfaceModule.CursorHandler;
            LockTextWriter writer = m_userInterfaceModule.OutputWriter;

            string readText = chatPacket.Message;
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

        public override void Stop()
        {
            base.Stop();
            CurrentLogic = loaderLogic;
        }
    }
}
