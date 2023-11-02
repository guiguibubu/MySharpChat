using MySharpChat.Core.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows;

namespace MySharpChat.Client.GUI
{
    internal class ChatViewModel : INotifyPropertyChanged
    {
        public GuiClientImpl Client { get; }
        public ObservableCollection<string> Messages { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> Users { get; } = new ObservableCollection<string>();
        public string InputMessage { get; set; } = "";

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged(string? propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChangedEventArgs e = new PropertyChangedEventArgs(propertyName);
                PropertyChanged(this, e);
            }
        }

        private string _localUserName = "";
        public string LocalUserName
        {
            get { return _localUserName; }
            set { 
                _localUserName = value;
                OnPropertyChanged(nameof(LocalUserName));
            }
        }

        public ChatViewModel(GuiClientImpl client)
        {
            Client = client;
            Client.DisconnectionEvent += OnDisconnection;
            Client.OnUserAddedEvent += OnUserAdded;
            Client.OnUserRemovedEvent += OnUserRemoved;
            Client.OnUsernameChangeEvent += OnUsernameChange;
            Client.OnLocalUsernameChangeEvent += OnLocalUsernameChange;
            Client.ChatMessageReceivedEvent += OnMessageReceived;
        }

        public event Action<bool> OnDisconnectionEvent = (bool manual) => { };
        public event Action<string, string> OnUsernameChangeEvent = (string s1, string s2) => { };
        public event Action<string> OnMessageReceivedEvent = (string message) => { };
        public event Action OnSendFinishedEvent = () => { };

        public void OnSendSuccess()
        {
            OnSendFinishedEvent();
        }

        public void OnDisconnection(bool manual)
        {
            OnDisconnectionEvent(manual);
        }

        public void OnUserAdded(string username)
        {
            Dispatcher uiDispatcher = Application.Current.Dispatcher;
            if (uiDispatcher.CheckAccess())
            {
                Users.Add(username);
            }
            else
            {
                uiDispatcher.Invoke(() => OnUserAdded(username));
            }
        }
        
        public void OnUserRemoved(string username)
        {
            Dispatcher uiDispatcher = Application.Current.Dispatcher;
            if (uiDispatcher.CheckAccess())
            {
                Users.Remove(username);
            }
            else
            {
                uiDispatcher.Invoke(() => OnUserRemoved(username));
            }
        }

        public void OnUsernameChange(string oldUsername, string newUsername)
        {
            OnUsernameChangeEvent(oldUsername, newUsername);
        }

        public void OnLocalUsernameChange()
        {
            LocalUserName = Client.LocalUser.Username;
        }

        public void OnMessageReceived(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                Messages.Add(message);
                OnMessageReceivedEvent(message);
            }
        }
    }
}
