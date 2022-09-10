using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using MySharpChat.Client.GUI.MAUI.Commands;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MySharpChat.Client.GUI.MAUI
{
    /// <summary>
    /// Interaction logic for ChatUserControl.xaml
    /// </summary>
    public partial class ChatUserControl : ContentView
    {
        private readonly ChatViewModel m_viewModel;

        private readonly List<Entry> usersUiElements = new List<Entry>();

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

            m_viewModel.OnDisconnectionEvent += OnDisconnection;
            m_viewModel.OnUserRemovedEvent += OnUsernameRemoved;
            m_viewModel.OnUserAddedEvent += OnUsernameAdded;
            m_viewModel.OnUsernameChangeEvent += OnUsernameChange;
            m_viewModel.OnLocalUsernameChangeEvent += OnLocalUsernameChange;
            m_viewModel.OnMessageReceivedEvent += OnMessageReceived;
            m_viewModel.OnSendFinishedEvent += OnSendFinished;
        }

        private void OnLocalUsernameChange()
        {
            if (MainThread.IsMainThread)
            {
                UserName.TextColor = Colors.Black;
                UserName.Text = m_viewModel.Client.LocalUser.Username;
                ConnectionStatus.TextColor = Colors.LimeGreen;
                ConnectionStatus.Text = "Connected !";
            }
            else
            {
                MainThread.InvokeOnMainThreadAsync(() => OnLocalUsernameChange());
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

                    OnUserStatusChange($"User leave the session : {username}");
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

                OnUserStatusChange($"New user joined : {username}");
            }
            else
            {
                MainThread.InvokeOnMainThreadAsync(() => OnUsernameAdded(username));
            }
        }

        private void OnUsernameChange(string oldUsername, string newUsername)
        {
            if (MainThread.IsMainThread)
            {
                Entry? userUiElement = usersUiElements.FirstOrDefault((ui) => ui.Text == oldUsername);
                if (userUiElement != null)
                {
                    userUiElement.Text = newUsername;

                    OnUserStatusChange($"Username change from {oldUsername} to {newUsername}");
                }
            }
            else
            {
                MainThread.InvokeOnMainThreadAsync(() => OnUsernameChange(oldUsername, newUsername));
            }
        }

        private void OnMessageReceived(string message)
        {
            if (MainThread.IsMainThread)
            {
                string text = message;
                if (!string.IsNullOrEmpty(text))
                {
                    Label outpuBlock = new Label();
                    outpuBlock.LineBreakMode = LineBreakMode.TailTruncation;
                    outpuBlock.Margin = new Thickness(0, 2, 0, 2);
                    outpuBlock.HorizontalTextAlignment = TextAlignment.Start;
                    outpuBlock.VerticalTextAlignment = TextAlignment.Center;
                    outpuBlock.BackgroundColor = Colors.WhiteSmoke;
                    outpuBlock.TextColor = Colors.Black;
                    outpuBlock.Text = message;

                    OutputStack.AddLogicalChild(outpuBlock);
                    OutputStack.ScrollTo(outpuBlock, null, ScrollToPosition.End);
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

        private void OnUserStatusChange(string message)
        {
            Label outpuBlock = new Label();
            outpuBlock.LineBreakMode = LineBreakMode.TailTruncation;
            outpuBlock.Margin = new Thickness(0, 2, 0, 2);
            outpuBlock.HorizontalTextAlignment = TextAlignment.Center;
            outpuBlock.VerticalTextAlignment = TextAlignment.Center;
            outpuBlock.BackgroundColor = Colors.WhiteSmoke;
            outpuBlock.TextColor = Colors.Black;
            outpuBlock.Text = message;

            OutputStack.AddLogicalChild(outpuBlock);
            OutputStack.ScrollTo(outpuBlock, null, ScrollToPosition.End);
        }

        private void OnInputCompleted(object? sender, EventArgs e)
        {
            SendButton.Command.Execute(SendButton.CommandParameter);
        }
    }
}
