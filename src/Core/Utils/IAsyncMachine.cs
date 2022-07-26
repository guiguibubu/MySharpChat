namespace MySharpChat.Core.Utils
{
    public interface IAsyncMachine
    {
        public void Initialize(object? initObject = null);
        public bool Start(object? startObject = null);
        public bool IsRunning();
        public void Stop(int exitCode = 0);
        public void Wait();
        public bool Wait(int millisecondsTimeout);

        public int ExitCode { get; }
    }
}
