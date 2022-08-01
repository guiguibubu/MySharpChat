using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySharpChat.Client.GUI
{
    internal class MainWindowViewModel
    {
        public GuiClientImpl Client { get; private set; }
        public StringWriter Output { get; private set; }

        public MainWindowViewModel(GuiClientImpl client, StringWriter output)
        {
            Client = client;
            Output = output;
        }
    }
}
