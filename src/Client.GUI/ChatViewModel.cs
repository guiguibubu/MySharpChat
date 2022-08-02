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
            Client.ChatMessageReceivedEvent += OnMessageReceived;
        }

        public event Action<string> OnMessageReceivedEvent = (string message) => { };
        public event Action OnSendFinishedEvent = () => { };

        public void OnSendSuccess()
        {
            OnSendFinishedEvent();
            
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
