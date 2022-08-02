using MySharpChat.Client.Command;
using MySharpChat.Client.Console.Command;
using MySharpChat.Client.Console.UI;
using MySharpChat.Client.Console.Input;
using MySharpChat.Core.Command;
using MySharpChat.Core.Packet;
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

        protected readonly ConsoleCommandInput commandInput;

        private readonly ReadingState readingState;

        public ConsoleClientImpl() : base()
        {
            m_userInterfaceModule = new ConsoleUserInterfaceModule();
            readingState = new ReadingState(new UserInputTextHandler(), m_userInterfaceModule);
            commandInput = new ConsoleCommandInput(m_userInterfaceModule);
            loaderLogic = new LoaderClientLogic(this);
            CurrentLogic = loaderLogic;
        }

        public override void Run(Client client)
        {
            // TODO reorganise to support read/write from network while reading inputs
            LockTextWriter writer = m_userInterfaceModule.OutputWriter;
            string currentPrefix = CurrentLogic.Prefix;
            UpdateInputLine(currentPrefix, "");

            Task<string> userInputTask = commandInput.ReadLineAsync(readingState);

            if (networkModule.IsConnected())
            {
                while (!userInputTask.Wait(TimeSpan.FromSeconds(1)))
                {
                    bool prefixChanged = false;
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
                                    {
                                        Username = newUsername;
                                        prefixChanged = true;
                                    }
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
                            else if(packet.Package is UserStatusPacket userStatusPackage)
                            {
                                HandleUserStatusPacket(userStatusPackage);
                            }
                        }
                    }

                    if (prefixChanged)
                    {
                        UpdateInputLine(currentPrefix, readingState.InputTextHandler.ToString());
                    }
                }
            }
            else
            {
                userInputTask.Wait();
            }

            CommandParser parser = CurrentLogic.CommandParser;
            string text = userInputTask.Result;

            CleanInputLine(currentPrefix);

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
        }

        private void HandleChatPacket(ChatPacket chatPacket)
        {
            string readText = chatPacket.Message;
            if (!string.IsNullOrEmpty(readText))
            {
                MoveInputLineDown(readText);
            }
        }

        private void HandleUserStatusPacket(UserStatusPacket userStatusPacket)
        {
            string username = userStatusPacket.Username;
            bool isConnected = userStatusPacket.Connected;
            string text = isConnected ? $"New user joined : {username}" : $"User leave the session : {username}";
            if (!string.IsNullOrEmpty(text))
            {
                MoveInputLineDown(text);
            }
        }


        public override void Stop()
        {
            base.Stop();
            CurrentLogic = loaderLogic;
        }

        private void MoveInputLineDown(string text)
        {
            IUserInputCursorHandler cursorHandler = m_userInterfaceModule.CursorHandler;
            LockTextWriter writer = m_userInterfaceModule.OutputWriter;

            using (writer.Lock())
            {
                cursorHandler.MovePositionToOrigin(CursorUpdateMode.GraphicalOnly);
                int prefixLength = CurrentLogic.Prefix.Length;
                cursorHandler.MovePositionNegative(prefixLength, CursorUpdateMode.GraphicalOnly);
                int inputTextLength = cursorHandler.Position;
                for (int i = 0; i < prefixLength + inputTextLength; i++)
                    writer.Write(" ");
                cursorHandler.MovePositionNegative(prefixLength + inputTextLength, CursorUpdateMode.GraphicalOnly);
                writer.WriteLine(text);
                writer.Write(CurrentLogic.Prefix);
            }
        }

        private void UpdateInputLine(string oldPrefix, string currentInputText)
        {
            LockTextWriter writer = m_userInterfaceModule.OutputWriter;
            using (writer.Lock())
            {
                CleanInputLine(oldPrefix);
                writer.Write(CurrentLogic.Prefix);
                writer.Write(currentInputText);
            }
        }

        private void CleanInputLine(string oldPrefix)
        {
            ConsoleCommandInput.ClearCommand(readingState);
            CleanPrefix(oldPrefix);
        }

        private void CleanPrefix(string prefix)
        {
            IUserInputCursorHandler cursorHandler = m_userInterfaceModule.CursorHandler;
            LockTextWriter writer = m_userInterfaceModule.OutputWriter;
            using (writer.Lock())
            {
                int prefixLength = prefix.Length;
                System.Console.CursorLeft = 0;
                for (int i = 0; i < prefixLength; i++)
                {
                    writer.Write(' ');
                }
                cursorHandler.MovePositionNegative(prefixLength, CursorUpdateMode.GraphicalOnly);
            }
        }
    }
}
