using MySharpChat.Client.GUI.Commands;
using MySharpChat.Core.Event;
using MySharpChat.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace MySharpChat.Client.GUI
{
    /// <summary>
    /// Interaction logic for ChatUserControl.xaml
    /// </summary>
    public partial class ChatUserControl : UserControl
    {
        private readonly ChatViewModel m_viewModel;

        private readonly Dictionary<Guid, TextBlock> usersUiElements = new();
        private readonly Dictionary<Guid, UIElement> chatUiElements = new();

        internal ChatUserControl(ChatViewModel viewModel)
        {
            InitializeComponent();

            m_viewModel = viewModel;

            InputBox.KeyDown += InputBox_KeyDown;
            InputBox.TextChanged += (object sender, TextChangedEventArgs e) => { m_viewModel.InputMessage = InputBox.Text; };
            SendButton.Command = new WpfSendCommand();
            SendButton.CommandParameter = new WpfSendArgs() { ViewModel = m_viewModel };

            DisconnectButton.Command = new WpfDisconnectCommand();
            DisconnectButton.CommandParameter = new WpfDisconnectionArgs() { ViewModel = m_viewModel };

            m_viewModel.OnDisconnectionEvent += OnDisconnection;
            m_viewModel.OnUserRemovedEvent += OnUsernameRemoved;
            m_viewModel.OnUserAddedEvent += OnUsernameAdded;
            m_viewModel.OnLocalUsernameChangeEvent += OnLocalUsernameChange;
            m_viewModel.OnUsernameChangeEvent += OnUsernameChange;
            m_viewModel.OnMessageReceivedEvent += OnMessageReceived;
            m_viewModel.OnSendFinishedEvent += OnSendFinished;

            DataContext = m_viewModel;
        }

        private void OnLocalUsernameChange()
        {
            UpdateUI();
        }

        public event Action<bool> OnDisconnectionEvent = (bool manual) => { };

        private void OnUsernameRemoved(Guid idUser)
        {
            UpdateUI();
        }

        private void OnUsernameChange(Guid idUser, string oldUsername, string newUsername)
        {
            UpdateUI();
        }

        private void OnUsernameAdded(Guid idUser)
        {
            UpdateUI();
        }

        private void OnMessageReceived(Guid idMessage)
        {
            UpdateUI();
        }

        private void OnDisconnection(bool manual)
        {
            Dispatcher uiDispatcher = Application.Current.Dispatcher;
            if (uiDispatcher.CheckAccess())
            {
                if (!manual)
                    MessageBox.Show(Application.Current.MainWindow, "Server connection lost. You will be disconnected.");
                OnDisconnectionEvent(manual);
            }
            else
            {
                uiDispatcher.Invoke(() => OnDisconnection(manual));
            }
        }

        private void OnSendFinished()
        {
            InputBox.Text = "";
            InputBox.Focus();
        }

        private void OnUserStatusChange(Guid idEvent, string message)
        {
            TextBlock outpuBlock = new TextBlock();
            outpuBlock.TextWrapping = TextWrapping.Wrap;
            outpuBlock.Margin = new Thickness(0, 2, 0, 2);
            outpuBlock.HorizontalAlignment = HorizontalAlignment.Center;
            outpuBlock.VerticalAlignment = VerticalAlignment.Center;
            outpuBlock.Background = new SolidColorBrush(Colors.WhiteSmoke);
            outpuBlock.Text = message;

            if(!chatUiElements.TryAdd(idEvent, outpuBlock))
            {
                chatUiElements[idEvent] = outpuBlock;
            }
        }

        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            Key key = e.Key;
            if (key == Key.Enter)
                SendButton.Command.Execute(SendButton.CommandParameter);
        }

        private void UpdateUI()
        {
            Dispatcher uiDispatcher = Application.Current.Dispatcher;
            if (uiDispatcher.CheckAccess())
            {
                IOrderedEnumerable<ChatEvent> chatEvents = m_viewModel.Client.ChatEvents.OrderedList;
                foreach (ChatEvent chatEvent in chatEvents)
                {
                    if (chatEvent is ChatMessageEvent chatMessageEvent)
                    {
                        HandleChatMessageEvent(chatMessageEvent);
                    }
                    if (chatEvent is ConnexionEvent connexionEvent)
                    {
                        HandleConnexionEvent(connexionEvent);
                    }
                    if (chatEvent is DisconnexionEvent disconnexionEvent)
                    {
                        HandleDisconnexionEvent(disconnexionEvent);
                    }
                    if (chatEvent is UsernameChangeEvent userChangeEvent)
                    {
                        HandleUsernameChangeEvent(userChangeEvent);
                    }
                }

                OutputStack.Children.Clear();
                foreach (UIElement uIElement in chatUiElements.Values)
                {
                    OutputStack.Children.Add(uIElement);
                }
                OutputScroller.ScrollToEnd();
            }
            else
            {
                uiDispatcher.Invoke(() => UpdateUI());
            }
        }

        private void HandleChatMessageEvent(ChatMessageEvent chatEvent)
        {
            ArgumentNullException.ThrowIfNull(chatEvent);

            ChatMessage chatMessage = chatEvent.ChatMessage;
            string username = chatMessage.User.Username;
            string messageText = chatMessage.Message;
            string text = $"({chatMessage.Date}) {username} : {messageText}";

            TextBlock outpuBlock = new TextBlock();
            outpuBlock.TextWrapping = TextWrapping.Wrap;
            outpuBlock.Margin = new Thickness(0, 2, 0, 2);
            outpuBlock.HorizontalAlignment = HorizontalAlignment.Stretch;
            outpuBlock.VerticalAlignment = VerticalAlignment.Center;
            outpuBlock.Background = new SolidColorBrush(Colors.WhiteSmoke);
            outpuBlock.Text = text;

            if(!chatUiElements.TryAdd(chatMessage.Id, outpuBlock))
            {
                chatUiElements[chatMessage.Id] = outpuBlock;
            }
        }

        private void HandleConnexionEvent(ConnexionEvent connexionEvent)
        {
            ArgumentNullException.ThrowIfNull(connexionEvent);

            string username = connexionEvent.User.Username;
            TextBlock userUiElement = new TextBlock() { Text = username, TextAlignment = TextAlignment.Center, TextWrapping = TextWrapping.Wrap };

            if (!usersUiElements.TryAdd(connexionEvent.Id, userUiElement))
            {
                TextBlock currentUiElement = usersUiElements[connexionEvent.Id];
                UsersStack.Children.Remove(currentUiElement);
                usersUiElements[connexionEvent.Id] = userUiElement;
            }
            UsersStack.Children.Add(userUiElement);

            OnUserStatusChange(connexionEvent.Id, $"New user joined : {username}");

            if (m_viewModel.Client.LocalUser.Id == connexionEvent.User.Id)
            {
                UserName.Foreground = new SolidColorBrush(Colors.Black);
                UserName.Text = m_viewModel.Client.LocalUser.Username;
                ConnectionStatus.Foreground = new SolidColorBrush(Colors.LimeGreen);
                ConnectionStatus.Text = "Connected !";
            }
        }

        private void HandleDisconnexionEvent(DisconnexionEvent disconnexionEvent)
        {
            ArgumentNullException.ThrowIfNull(disconnexionEvent);
            User user = disconnexionEvent.User;
            if (usersUiElements.TryGetValue(user.Id, out TextBlock? userUiElement))
            {
                UsersStack.Children.Remove(userUiElement);
                usersUiElements.Remove(user.Id);

                OnUserStatusChange(disconnexionEvent.Id, $"User leave the session : {user.Username}");
            }
        }

        private void HandleUsernameChangeEvent(UsernameChangeEvent userChangeEvent)
        {
            ArgumentNullException.ThrowIfNull(userChangeEvent);

            string oldUsername = userChangeEvent.OldUsername;
            string newUsername = userChangeEvent.NewUsername;

            if (usersUiElements.TryGetValue(userChangeEvent.UidUser, out TextBlock? userUiElement))
            {
                userUiElement.Text = newUsername;

                OnUserStatusChange(userChangeEvent.Id, $"Username change from {oldUsername} to {newUsername}");
            }
        }
    }
}
