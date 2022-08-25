using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySharpChat.Client.GUI.MAUI
{
    internal class ConnectionViewModel
    {
        public GuiClientImpl Client { get; private set; }
        public string Username { get; set; } = "";
        public string ServerIp { get; set; } = "";
        public string ServerPort { get; set; } = "";

        internal ConnectionViewModel(GuiClientImpl client)
        {
            Client = client;
        }

        public event Action<bool> OnDisconnectionEvent = (bool manual) => { };
        public event Action OnConnectionSuccessEvent = () => { };
        public event Action OnConnectionFailEvent = () => { };

        public void OnDisconnection(bool manual)
        {
            OnDisconnectionEvent(manual);
        }

        public void OnConnectionSuccess()
        {
            OnConnectionSuccessEvent();
        }

        public void OnConnectionFail()
        {
            OnConnectionFailEvent();
        }
    }
}
