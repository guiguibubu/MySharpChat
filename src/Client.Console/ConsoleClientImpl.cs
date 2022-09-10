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
using MySharpChat.Core.Http;
using System.Linq;
using MySharpChat.Core.Model;
using static System.Net.Mime.MediaTypeNames;

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

            while (!userInputTask.Wait(TimeSpan.FromSeconds(1)))
            {
                if (networkModule.IsConnected())
                {
                    bool prefixChanged = false;
                    int nbPacketsHandles = 0;
                    while (networkModule.HasDataAvailable && nbPacketsHandles < 100)
                    {
                        HandleNetworkPackets();
                    }

                    if (prefixChanged)
                    {
                        UpdateInputLine(currentPrefix, readingState.InputTextHandler.ToString());
                    }
                }
                else
                {
                    if (CurrentLogic is ChatClientLogic)
                    {
                        writer.WriteLine("Connection to server lost. Will disconnect.");
                        Task.Delay(3000).GetAwaiter().GetResult();
                        ConsoleDisconnectCommand? disconnectCommand = CurrentLogic.CommandParser.Parse<ConsoleDisconnectCommand>(DisconnectCommand.Instance.Name);
                        disconnectCommand!.Execute(this);
                        return;
                    }
                }
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

        private void HandleNetworkPackets()
        {
            ClientNetworkModule clientNetworkModule = ((ClientNetworkModule)networkModule);
            PacketWrapper packet = clientNetworkModule.CurrentPacket;
            if (packet.Package is UserInfoPacket userInfoPacket)
            {
                User user = userInfoPacket.User;
                Guid userId = user.Id;
                string username = user.Username;

                if(LocalUser.Id == userId)
                    LocalUser.Username = username;

                bool knownUser = ChatRoom!.Users.Contains(userId);
                bool isConnected = userInfoPacket.Connected;
                bool isDisconnection = knownUser && !isConnected;
                if (isDisconnection)
                {
                    ChatRoom!.Users.Remove(userId);
                    string text = $"User leave the session : {username}";
                    MoveInputLineDown(text);
                    return;
                }

                bool alreadyDiconnected = !knownUser && !isConnected;
                if (alreadyDiconnected)
                    return;

                bool newUser = !knownUser && isConnected;
                if (newUser)
                {
                    ChatRoom!.Users.Add(new UserState(user, true));
                    string text = $"New user joined : {username}";
                    MoveInputLineDown(text);
                    return;
                }

                User userInCache = ChatRoom!.Users[userId].User;
                string oldUsername = userInCache.Username;
                if (oldUsername != username)
                {
                    string text = $"Username change from {oldUsername} to {username}";
                    userInCache.Username = username;
                    MoveInputLineDown(text);
                }
            }
            else if (packet.Package is ChatPacket chatPackage)
            {
                HandleChatPacket(chatPackage);
            }
        }

        private void HandleChatPacket(ChatPacket chatPacket)
        {
            ChatMessage chatMessage = chatPacket.ChatMessage;
            if (!ChatRoom!.Messages.Contains(chatMessage))
            {
                ChatRoom!.Messages.Add(chatMessage);
                string username = chatMessage.User.Username;
                string messageText = chatMessage.Message;
                string readText = $"({chatMessage.Date}) {username} : {messageText}";
                MoveInputLineDown(readText);
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
