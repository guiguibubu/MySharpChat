using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySharpChat.Client.GUI
{
    internal class ChatViewModel
    {
        public GuiClientImpl Client { get; private set; }

        public ChatViewModel(GuiClientImpl client)
        {
            Client = client;
        }
    }
}
