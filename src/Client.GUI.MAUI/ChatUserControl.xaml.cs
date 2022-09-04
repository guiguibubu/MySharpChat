using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using MySharpChat.Client.Command;
using MySharpChat.Client.GUI.MAUI.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace MySharpChat.Client.GUI.MAUI
{
    /// <summary>
    /// Interaction logic for ChatUserControl.xaml
    /// </summary>
    public partial class ChatUserControl : ContentView
    {
        private readonly ChatViewModel m_viewModel;

        private readonly List<Entry> usersUiElements = new List<Entry>();
        private readonly ObservableCollection<string> messages = new ObservableCollection<string>();

        internal ChatUserControl(ChatViewModel viewModel)
        {
            InitializeComponent();

            m_viewModel = viewModel;

            InputBox.Completed += OnInputCompleted;
            InputBox.TextChanged += (object? sender, TextChangedEventArgs e) => { m_viewModel.InputMessage = InputBox.Text; };
            SendButton.Command = new WpfSendCommand();
            SendButton.CommandParameter = new WpfSendArgs() { ViewModel = m_viewModel };

            DisconnectButton.Command = new WpfDisconnectCommand();
            DisconnectButton.CommandParameter = new WpfDisconnectionArgs() { ViewModel = m_viewModel };

            OutputStack.ItemsSource = messages;

            m_viewModel.OnDisconnectionEvent += OnDisconnection;
            m_viewModel.OnUserRemovedEvent += OnUsernameRemoved;
            m_viewModel.OnUserAddedEvent += OnUsernameAdded;
            m_viewModel.OnUsernameChangeEvent += OnUsernameChange;
            m_viewModel.OnMessageReceivedEvent += OnMessageReceived;
            m_viewModel.OnSendFinishedEvent += OnSendFinished;
        }

        private void OnUsernameChange()
        {
            if (MainThread.IsMainThread)
            {
                UserName.TextColor = Colors.Black;
                UserName.Text = m_viewModel.Client.Username;
                ConnectionStatus.TextColor = Colors.LimeGreen;
                ConnectionStatus.Text = "Connected !";
            }
            else
            {
                MainThread.InvokeOnMainThreadAsync(() => OnUsernameChange());
            }
        }

        public event Action<bool> OnDisconnectionEvent = (bool manual) => { };

        private void OnUsernameRemoved(string username)
        {
            if (MainThread.IsMainThread)
            {
                Entry? userUiElement = usersUiElements.FirstOrDefault((ui) => ui.Text == username);
                if (userUiElement != null)
                {
                    usersUiElements.Remove(userUiElement);
                    UsersStack.Children.Remove(userUiElement);
                }
            }
            else
            {
                MainThread.InvokeOnMainThreadAsync(() => OnUsernameRemoved(username));
            }
        }

        private void OnUsernameAdded(string username)
        {
            if (MainThread.IsMainThread)
            {
                Entry userUiElement = new Entry() { Text = username, HorizontalTextAlignment = TextAlignment.Center, VerticalTextAlignment = TextAlignment.Center };
                usersUiElements.Add(userUiElement);
                UsersStack.Children.Add(userUiElement);
            }
            else
            {
                MainThread.InvokeOnMainThreadAsync(() => OnUsernameAdded(username));
            }
        }

        private void OnMessageReceived(string message)
        {
            if (MainThread.IsMainThread)
            {
                string text = message;
                if (!string.IsNullOrEmpty(text))
                {
                    messages.Add(text);
                }
            }
            else
            {
                MainThread.InvokeOnMainThreadAsync(() => OnMessageReceived(message));
            }
        }

        private void OnDisconnection(bool manual)
        {
            if (MainThread.IsMainThread)
            {
                if (!manual)
                    Application.Current?.MainPage?.DisplayAlert("Connection lost", "Server connection lost. You will be disconnected.", "OK");
                OnDisconnectionEvent(manual);
            }
            else
            {
                MainThread.InvokeOnMainThreadAsync(() => OnDisconnection(manual));
            }
        }

        private void OnSendFinished()
        {
            if (MainThread.IsMainThread)
            {
                InputBox.Text = "";
                InputBox.Focus();
            }
            else
            {
                MainThread.InvokeOnMainThreadAsync(() => OnSendFinished());
            }
        }

        private void OnInputCompleted(object? sender, EventArgs e)
        {
            SendButton.Command.Execute(SendButton.CommandParameter);
        }
    }
}
