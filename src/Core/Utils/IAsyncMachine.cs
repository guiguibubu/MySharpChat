namespace MySharpChat.Core.Utils
{
    public interface IAsyncMachine
    {
        public void Initialize(object? initObject = null);
        public void InitCommands();
        public bool Start(object? startObject = null);
        public bool IsRunning();
        public bool IsConnected(ConnexionInfos? connexionInfos = null);
        public void Stop(int exitCode = 0);
        public void Wait();
        public bool Wait(int millisecondsTimeout);

        public int ExitCode { get; }

        public bool Connect(ConnexionInfos connexionInfos);
        public void Send(string? text);
        public string Read();
        public void Disconnect(ConnexionInfos? connexionInfos);
    }
}
