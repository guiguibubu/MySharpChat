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
        }

        public event Action OnSendSuccessEvent = () => { };

        public void OnSendSuccess()
        {
            string text = InputMessage;
            if (!string.IsNullOrEmpty(text))
            {
                Messages.Add(text);
                OnSendSuccessEvent();
            }
        }
    }
}
