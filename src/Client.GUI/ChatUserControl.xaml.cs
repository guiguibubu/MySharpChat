using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using MySharpChat.Client.GUI.Commands;

namespace MySharpChat.Client.GUI
{
    /// <summary>
    /// Interaction logic for ChatUserControl.xaml
    /// </summary>
    public partial class ChatUserControl : UserControl
    {
        private readonly ChatViewModel m_viewModel;

        private readonly List<TextBlock> usersUiElements = new List<TextBlock>();

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
            Dispatcher uiDispatcher = Application.Current.Dispatcher;
            if (uiDispatcher.CheckAccess())
            {
                UserName.Foreground = new SolidColorBrush(Colors.Black);
                UserName.Text = m_viewModel.Client.LocalUser.Username;
                ConnectionStatus.Foreground = new SolidColorBrush(Colors.LimeGreen);
                ConnectionStatus.Text = "Connected !";
            }
            else
            {
                uiDispatcher.Invoke(OnLocalUsernameChange);
            }
        }

        public event Action<bool> OnDisconnectionEvent = (bool manual) => { };

        private void OnUsernameRemoved(string username)
        {
            Dispatcher uiDispatcher = Application.Current.Dispatcher;
            if (uiDispatcher.CheckAccess())
            {
                TextBlock? userUiElement = usersUiElements.FirstOrDefault((ui) => ui.Text == username);
                if (userUiElement != null)
                {
                    usersUiElements.Remove(userUiElement);
                    UsersStack.Children.Remove(userUiElement);
                    OnUserStatusChange($"User leave the session : {username}");
                }
            }
            else
            {
                uiDispatcher.Invoke(OnUsernameRemoved, username);
            }
        }

        private void OnUsernameChange(string oldUsername, string newUsername)
        {
            Dispatcher uiDispatcher = Application.Current.Dispatcher;
            if (uiDispatcher.CheckAccess())
            {
                TextBlock? userUiElement = usersUiElements.FirstOrDefault((ui) => ui.Text == oldUsername);
                if (userUiElement != null)
                {
                    userUiElement.Text = newUsername;

                    OnUserStatusChange($"Username change from {oldUsername} to {newUsername}");
                }
            }
            else
            {
                uiDispatcher.Invoke(OnUsernameChange, oldUsername, newUsername);
            }
        }

        private void OnUsernameAdded(string username)
        {
            Dispatcher uiDispatcher = Application.Current.Dispatcher;
            if (uiDispatcher.CheckAccess())
            {
                TextBlock userUiElement = new TextBlock() { Text = username, TextAlignment = TextAlignment.Center, TextWrapping = TextWrapping.Wrap };
                usersUiElements.Add(userUiElement);
                UsersStack.Children.Add(userUiElement);

                OnUserStatusChange($"New user joined : {username}");
            }
            else
            {
                uiDispatcher.Invoke(OnUsernameAdded, username);
            }
        }

        private void OnMessageReceived(string message)
        {
            string text = message;
            if (!string.IsNullOrEmpty(text))
            {
                Dispatcher uiDispatcher = Application.Current.Dispatcher;
                if (uiDispatcher.CheckAccess())
                {
                    TextBlock outpuBlock = new TextBlock();
                    outpuBlock.TextWrapping = TextWrapping.Wrap;
                    outpuBlock.Margin = new Thickness(0, 2, 0, 2);
                    outpuBlock.HorizontalAlignment = HorizontalAlignment.Stretch;
                    outpuBlock.VerticalAlignment = VerticalAlignment.Center;
                    outpuBlock.Background = new SolidColorBrush(Colors.WhiteSmoke);
                    outpuBlock.Text = text;

                    OutputStack.Children.Add(outpuBlock);
                    OutputScroller.ScrollToEnd();
                }
                else
                {
                    uiDispatcher.Invoke(() => OnMessageReceived(text));
                }
            }
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

        private void OnUserStatusChange(string message)
        {
            TextBlock outpuBlock = new TextBlock();
            outpuBlock.TextWrapping = TextWrapping.Wrap;
            outpuBlock.Margin = new Thickness(0, 2, 0, 2);
            outpuBlock.HorizontalAlignment = HorizontalAlignment.Center;
            outpuBlock.VerticalAlignment = VerticalAlignment.Center;
            outpuBlock.Background = new SolidColorBrush(Colors.WhiteSmoke);
            outpuBlock.Text = message;

            OutputStack.Children.Add(outpuBlock);
            OutputScroller.ScrollToEnd();
        }

        private void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            Key key = e.Key;
            if (key == Key.Enter)
                SendButton.Command.Execute(SendButton.CommandParameter);
        }
    }
}
