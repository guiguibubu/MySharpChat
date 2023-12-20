using System;
using System.Threading;
using System.Diagnostics;

using MySharpChat.Core.Utils;
using MySharpChat.Core.Utils.Logger;

namespace MySharpChat.Client
{
    public partial class Client : IAsyncMachine
    {
        private readonly IClientImpl _clientImpl;

        public Client(IClientImpl clientImpl)
        {
            _clientImpl = clientImpl;
        }

        public int ExitCode { get; private set; }

        private bool m_clientRun = false;
        private SafeThread? m_clientThread = null;

        private bool _initialized = false;
        private static readonly Logger logger = Logger.Factory.GetLogger<Client>();

        public virtual void Initialize(object? initObject = null)
        {
            _clientImpl.Initialize(initObject);
        }

        public bool Start(object? startObject = null)
        {
            return _initialized = Start(startObject as string);
        }

        public virtual bool Start(string? serverAdress)
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw.Start();

            bool clientStarted = false;

            Initialize();

            m_clientThread = new SafeThread(Run);
            m_clientThread.Start();

            while (!m_clientRun && sw.Elapsed < TimeSpan.FromSeconds(1)) { Thread.SpinWait(100); }

            logger.LogInfo("Client started (in {0} ms) !", sw.ElapsedMilliseconds);

            clientStarted = true;

            return clientStarted;
        }

        private void Run()
        {
            if (Thread.CurrentThread.Name == null)
            {
                Thread.CurrentThread.Name = "RunningClientThread";
                logger.LogDebug(string.Format("{0} started (Thread {1})", Thread.CurrentThread.Name, Thread.CurrentThread.ManagedThreadId));
            }

            m_clientRun = true;
            while (m_clientRun)
            {
                _clientImpl.Run(this);
            }
        }

        public virtual bool IsRunning()
        {
            return m_clientRun;
        }

        public virtual void Stop(int exitCode = 0)
        {
            _clientImpl.Stop();
            m_clientRun = false;
            ExitCode = exitCode;
        }

        public virtual void Wait()
        {
            m_clientThread?.Join();
        }

        public bool Wait(int millisecondsTimeout)
        {
            return m_clientThread?.Join(millisecondsTimeout) ?? true;
        }

    }
}
