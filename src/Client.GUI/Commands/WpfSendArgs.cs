using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySharpChat.Client.GUI.Commands
{
    internal struct WpfSendArgs
    {
        public ChatUserControl chatUC;
        public IClientImpl client;
        public string[] args;
    }
}
