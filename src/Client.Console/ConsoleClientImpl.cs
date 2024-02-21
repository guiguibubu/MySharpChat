using MySharpChat.Client.Command;
using MySharpChat.Client.Console.Command;
using MySharpChat.Client.Console.UI;
using MySharpChat.Client.Console.Input;
using MySharpChat.Core.Command;
using MySharpChat.Core.Packet;
using MySharpChat.Core.Utils.Logger;
using System;
using System.Threading.Tasks;
using MySharpChat.Core.Model;
using System.Threading;
using MySharpChat.Core.Event;
using System.Collections.Generic;
using System.Linq;

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

        private CancellationTokenSource _cancellationSource = new CancellationTokenSource();
        private Task? _uiUpdateTask = null;

        public ConsoleClientImpl() : base()
        {
            m_userInterfaceModule = new ConsoleUserInterfaceModule();
            ConsoleCursorHandler cursorHandler = new ConsoleCursorHandler();
            readingState = new ReadingState(new UserInputTextHandler(), cursorHandler, m_userInterfaceModule);
            commandInput = new ConsoleCommandInput(m_userInterfaceModule, cursorHandler);
            commandInput.OnInputChanged += UpdateInputLine;
            loaderLogic = new LoaderClientLogic(this);
            CurrentLogic = loaderLogic;
        }

        public override void Initialize(object? initObject = null)
        {
            m_userInterfaceModule.OutputModule.Initialize();

            StartUiUpdater();
        }

        public override void Run(Client client)
        {
            // TODO reorganise to support read/write from network while reading inputs
            ConsoleOutputModule outputModule = m_userInterfaceModule.OutputModule;
            UpdateInputLine();

            Task<string> userInputTask = commandInput.ReadLineAsync(readingState);

            while (!userInputTask.Wait(TimeSpan.FromSeconds(1)))
            {
                if (networkModule.IsConnected())
                {
                    HandleNetworkPackets(100);
                }
                else
                {
                    if (CurrentLogic is ChatClientLogic)
                    {
                        string errorMessage = "Connection to server lost. Will disconnect.";
                        outputModule.WriteLineOutput(errorMessage);
                        Task.Delay(3000).GetAwaiter().GetResult();
                        ConsoleDisconnectCommand? disconnectCommand = CurrentLogic.CommandParser.Parse<ConsoleDisconnectCommand>(DisconnectCommand.Instance.Name);
                        disconnectCommand!.Execute(this);
                        return;
                    }
                }

                UpdateUI();
            }

            UpdateUI();

            CommandParser parser = CurrentLogic.CommandParser;
            string text = userInputTask.Result;

            CleanErrorBox();
            CleanInputLine();

            if (parser.TryParse(text, out string[] args, out IClientCommand? clientCommand))
            {
                if (!clientCommand!.Execute(this, args))
                {
                    outputModule.WriteLineError("Fail of command \"{0}\"", text);
                    HelpCommand helpCommand = parser.GetHelpCommand();
                    helpCommand.Execute(outputModule.ErrorWriter, clientCommand.Name);
                }
            }
            else if (parser.TryParse(text, out args, out IAsyncMachineCommand? asyncMachineCommand))
            {
                if (!asyncMachineCommand!.Execute(client, args))
                {
                    outputModule.WriteLineError("Fail of command \"{0}\"", text);
                    HelpCommand helpCommand = parser.GetHelpCommand();
                    helpCommand.Execute(outputModule.ErrorWriter, asyncMachineCommand.Name);
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
                    outputModule.WriteLineError("Fail of command \"{0}\"", text);
                    HelpCommand helpCommand = parser.GetHelpCommand();
                    helpCommand.Execute(outputModule.ErrorWriter, consoleCommand.Name);
                }
            }
            else
            {
                outputModule.WriteLineError("Fail to parse \"{0}\"", text);
                outputModule.WriteLineError();
                outputModule.WriteLineError("Available commands");
                parser.GetHelpCommand().Execute(outputModule.ErrorWriter);
            }
        }

        private void HandleNetworkPacket(PacketWrapper? packet)
        {
            if (packet != null && packet.Package is ChatEvent chatEvent)
            {
                HandleNetworkPacket(chatEvent);
            }
        }

        private void HandleNetworkPacket(ChatEvent chatEvent)
        {
            if (!ChatEvents.Contains(chatEvent.Id))
            {
                ChatEvents.Add(chatEvent);
            }
        }

        private void HandleNetworkPackets(int nbMaxPacket = int.MaxValue)
        {
            int nbPacketsHandles = 0;
            while (networkModule.HasDataAvailable && nbPacketsHandles < nbMaxPacket)
            {
                PacketWrapper? packet = networkModule.CurrentData;
                HandleNetworkPacket(packet);
            }
        }

        private static string ToString(ChatEvent chatEvent)
        {
            string text;

            if (chatEvent is ChatMessageEvent chatMessageEvent)
            {
                text = ToString(chatMessageEvent);
            }
            else if (chatEvent is ConnexionEvent connexionEvent)
            {
                text = ToString(connexionEvent);
            }
            else if (chatEvent is DisconnexionEvent disconnexionEvent)
            {
                text = ToString(disconnexionEvent);
            }
            else if (chatEvent is UsernameChangeEvent usernameChangeEvent)
            {
                text = ToString(usernameChangeEvent);
            }
            else
            {
                throw new NotImplementedException();
            }

            return text;
        }

        private static string ToString(ChatMessageEvent chatMessageEvent)
        {
            ChatMessage chatMessage = chatMessageEvent.ChatMessage;
            string username = chatMessage.User.Username;
            string messageText = chatMessage.Message;
            string readText = $"({chatMessage.Date}) {username} : {messageText}";
            return readText;
        }

        private static string ToString(ConnexionEvent connexionEvent)
        {
            User user = connexionEvent.User;
            string text = $"New user joined : {user.Username}";
            return text;
        }

        private static string ToString(DisconnexionEvent disconnexionEvent)
        {
            User user = disconnexionEvent.User;
            string text = $"User leave the session : {user.Username}";
            return text;
        }

        private static string ToString(UsernameChangeEvent usernameChangeEvent)
        {
            string oldUsername = usernameChangeEvent.OldUsername;
            string newUsername = usernameChangeEvent.NewUsername;
            string text = $"Username change from {oldUsername} to {newUsername}";
            return text;
        }

        private void StartUiUpdater()
        {
            if (_uiUpdateTask == null)
                _uiUpdateTask = Task.Run(() =>
                {
                    while (!_cancellationSource.IsCancellationRequested)
                    {
                        UiUpdateAction();
                    }
                }, _cancellationSource.Token);
        }

        private void StopUiUpdater()
        {
            _cancellationSource.Cancel();
            try
            {
                _uiUpdateTask?.Wait();
            }
            catch (AggregateException e)
            {
                if (!(e.InnerException is TaskCanceledException))
                    throw;
            }
            _cancellationSource = new CancellationTokenSource();
            _uiUpdateTask = null;
        }

        private void UiUpdateAction()
        {
            Task.Delay(TimeSpan.FromMilliseconds(100)).GetAwaiter().GetResult();
            m_userInterfaceModule.OutputModule.Refresh();
        }

        public override void Stop()
        {
            base.Stop();
            StopUiUpdater();
            CurrentLogic = loaderLogic;
            ConsoleOutputModule outputModule = m_userInterfaceModule.OutputModule;
            outputModule.Clear();
            outputModule.SetInputPrefix("");
            outputModule.SetInputPosition(0);
            outputModule.Refresh(true);
        }

        private void UpdateInputLine()
        {
            ConsoleOutputModule outputModule = m_userInterfaceModule.OutputModule;
            using (outputModule.Lock())
            {
                CleanInputLine();

                IUserInputTextHandler userInputTextHandler = readingState.InputTextHandler;
                ConsoleCursorHandler inputCursorHandler = readingState.InputCursorHandler;

                string currentInputText = userInputTextHandler.ToString();
                string currentPrefix = CurrentLogic.Prefix;
                int inputPosition = inputCursorHandler.Position;

                outputModule.SetInputPrefix(currentPrefix);
                outputModule.WriteInput(currentInputText);
                outputModule.SetInputPosition(inputPosition);
            }
        }

        private void CleanErrorBox()
        {
            m_userInterfaceModule.OutputModule.ClearError();
        }

        private void CleanInputLine()
        {
            ConsoleOutputModule outputModule = m_userInterfaceModule.OutputModule;
            outputModule.ClearInput();
            outputModule.SetInputPosition(0);
            outputModule.SetInputPrefix("");
        }

        private void UpdateUI()
        {
            ConsoleOutputModule outputModule = m_userInterfaceModule.OutputModule;
            IEnumerable<ChatEvent> eventsToShow = ChatEvents.OrderByDescending(chatEvent => chatEvent.Date);
            outputModule.ClearOutput();
            foreach (string message in eventsToShow.Select(chatEvent => ToString(chatEvent)))
            {
                outputModule.WriteLineOutput(message);
            }
        }
    }
}
