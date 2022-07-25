using System.Net.Sockets;

namespace MySharpChat.Core.Utils
{
    public interface IAsyncMachine
    {
        public void Initialize(object? initObject = null);
        public void InitCommands();
        public bool Start(object? startObject = null);
        public bool IsRunning();
        public bool IsConnected(Socket? socket);
        public void Stop(int exitCode = 0);
        public void Wait();
        public bool Wait(int millisecondsTimeout);

        public int ExitCode { get; }

        public bool Connect(ConnexionInfos connexionInfos);
        public void Disconnect(Socket? socket);
    }
}
