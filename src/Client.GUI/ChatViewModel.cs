using System;
using System.Collections.ObjectModel;

namespace MySharpChat.Client.GUI
{
    internal class ChatViewModel
    {
        public GuiClientImpl Client { get; private set; }
        public ObservableCollection<string> Messages { get; private set; } = new ObservableCollection<string>();
        public string InputMessage { get; set; } = "";

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
        public event Action<string> OnUserAddedEvent = (string s) => { };
        public event Action<string> OnUserRemovedEvent = (string s) => { };
        public event Action<string, string> OnUsernameChangeEvent = (string s1, string s2) => { };
        public event Action OnLocalUsernameChangeEvent = () => { };
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
            OnUserAddedEvent(username);
        }

        public void OnUserRemoved(string username)
        {
            OnUserRemovedEvent(username);
        }

        public void OnUsernameChange(string oldUsername, string newUsername)
        {
            OnUsernameChangeEvent(oldUsername, newUsername);
        }

        public void OnLocalUsernameChange()
        {
            OnLocalUsernameChangeEvent();
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
