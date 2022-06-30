using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySharpChat.Core.Utils
{
    public interface IAsyncMachine
    {
        public void Initialize(object? initObject = null);
        public void InitCommands();
        public bool Start(object? startObject = null);
        public bool IsRunning();
        public bool IsConnected(ConnexionInfos? connexionInfos);
        public void Stop();
        public void Wait();
        public bool Wait(int millisecondsTimeout);

        public bool Connect(ConnexionInfos connexionInfos);
        public void Send(string? text);
        public string Read();
        public void Disconnect(ConnexionInfos? connexionInfos);
    }
}
