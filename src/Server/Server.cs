﻿using System;
using System.Threading;
using System.Diagnostics;

using MySharpChat.Core.Command;
using MySharpChat.Core.Utils;
using MySharpChat.Core.Utils.Logger;

namespace MySharpChat.Server
{
    public class Server : IAsyncMachine
    {
        private static readonly Logger logger = Logger.Factory.GetLogger<Server>();

        private bool m_serverRun = false;
        private Thread? m_serverThread = null;

        private readonly CommandManager commandManager = new CommandManager();

        private readonly IServerImpl _serverImpl;

        public int ExitCode { get; private set; }

        public Server(IServerImpl serverImpl)
        {
            _serverImpl = serverImpl;
            Initialize();
        }

        ~Server()
        {
            Stop();
        }

        public void Initialize(object? initObject = null)
        {
            InitCommands();
        }

        public void InitCommands()
        {
        }

        public bool Start(object? startObject = null)
        {
            if (Thread.CurrentThread.Name is null)
            {
                Thread.CurrentThread.Name = "MainServerThread";
            }

            Stopwatch sw = Stopwatch.StartNew();

            bool serverStarted = false;
            try
            {
                m_serverThread = new Thread(Run);
                m_serverThread.Start();

                while (!m_serverRun && sw.Elapsed < TimeSpan.FromSeconds(1)) { Thread.SpinWait(100); }

                logger.LogInfo("Server started (in {0} ms) !", sw.ElapsedMilliseconds);

                serverStarted = true;

            }
            catch (Exception e)
            {
                logger.LogError("Fail to start server");
                logger.LogError(e.ToString());
            }

            return serverStarted;
        }

        public bool IsRunning()
        {
            return m_serverRun;
        }

        public void Stop(int exitCode = 0)
        {
            _serverImpl.Stop();
            m_serverRun = false;
            ExitCode = exitCode;
        }

        public void Wait()
        {
            m_serverThread?.Join();
        }

        public bool Wait(int millisecondsTimeout)
        {
            return m_serverThread?.Join(millisecondsTimeout) ?? true;
        }

        private void Run()
        {
            if (Thread.CurrentThread.Name is null)
            {
                Thread.CurrentThread.Name = "RunningServerThread";
                logger.LogDebug(string.Format("{0} started (Thread {1})", Thread.CurrentThread.Name, Thread.CurrentThread.ManagedThreadId));
            }

            _serverImpl.Start();
            m_serverRun = true;
            while (m_serverRun)
            {
                _serverImpl.Run(this);
            }

            logger.LogInfo("Server stopped !");
        }
    }
}
