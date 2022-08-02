using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            Client.DisconnectionEvent += (bool manualDisconnection) => OnDisconnectionEvent(manualDisconnection);
            Client.OnUsernameChangeEvent += OnUsernameChange;
            Client.ChatMessageReceivedEvent += OnMessageReceived;
        }

        public event Action<bool> OnDisconnectionEvent = (bool manual) => { };
        public event Action OnUsernameChangeEvent = () => { };
        public event Action<string> OnMessageReceivedEvent = (string message) => { };
        public event Action OnSendFinishedEvent = () => { };

        public void OnSendSuccess()
        {
            OnSendFinishedEvent();
            
        }

        public void OnUsernameChange()
        {
            OnUsernameChangeEvent();
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
