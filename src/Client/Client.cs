using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;

using MySharpChat.Core.Command;
using MySharpChat.Core.SocketModule;
using MySharpChat.Core.Utils;
using MySharpChat.Client.Command;
using MySharpChat.Core.Utils.Logger;
using System.Threading.Tasks;
using MySharpChat.Client.Input;
using System.IO;
using MySharpChat.Client.UI;

namespace MySharpChat.Client
{
    public partial class Client : IAsyncMachine
    {
        // TODO Refactor with modules (network, input, ui ...)
        private IClientImpl clientImp = new DefaultClientImpl();

        private static Client? instance = null;
        public static Client Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Client();
                }
                return instance;
            }
        }

        public int ExitCode { get; private set; }

        private bool m_clientRun = false;
        private Thread? m_clientThread = null;

        private static readonly Logger logger = Logger.Factory.GetLogger<Client>();

        public LockTextWriter OutputWriter => clientImp.OutputWriter;

        public virtual void Initialize(object? initObject = null)
        {
        }

        public bool Start(object? startObject = null)
        {
            return Start(startObject as string);
        }

        public virtual bool Start(string? serverAdress)
        {
            Stopwatch sw = Stopwatch.StartNew();
            sw.Start();

            bool clientStarted = false;

            try
            {
                m_clientThread = new Thread(Run);
                m_clientThread.Start();

                while (!m_clientRun && sw.Elapsed < TimeSpan.FromSeconds(1)) { Thread.SpinWait(100); }

                logger.LogInfo("Client started (in {0} ms) !", sw.ElapsedMilliseconds);

                clientStarted = true;

            }
            catch (Exception e)
            {
                OutputWriter.WriteLine(e.ToString());
            }

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
                clientImp.Run(this);
            }

            OutputWriter.WriteLine("Client stopped !");
        }

        public virtual bool IsRunning()
        {
            return m_clientRun;
        }

        public virtual void Stop(int exitCode = 0)
        {
            clientImp.Stop();
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
