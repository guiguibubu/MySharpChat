using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MySharpChat.Client.Input
{
    public class ConsoleOutputWriter : LockTextWriter
    {
        private readonly TextWriter _output;

        private readonly Queue<char> _queue = new Queue<char>();
        private Scope? _scope = null;

        public string Prefix { get; set; } = "";

        public ConsoleOutputWriter(TextWriter output) : base()
        {
            _output = output;
        }
        public ConsoleOutputWriter() : this(Console.Out) { }

        public override Encoding Encoding => _output.Encoding;

        public override void Write(char value)
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;
            bool hasToWrite = !IsLocked || (IsLocked && _scope!.ThreadId == threadId);
            if (hasToWrite)
                _output.Write(value);
            else
                _queue.Enqueue(value);
        }

        public override IDisposable Lock()
        {
            int threadId = Thread.CurrentThread.ManagedThreadId;
            _scope = new Scope(threadId, DequeueChars);
            return _scope;
        }

        private void DequeueChars()
        {
            if (!IsLocked)
            {
                while (_queue.TryDequeue(out char c))
                {
                    Write(c);
                }
            }
        }

        private readonly object _lockObject = new object();
        public override bool IsLocked
        {
            get
            {
                lock (_lockObject)
                {
                    return _scope != null && _scope.IsActive;
                }
            }
        }


        private class Scope : IDisposable
        {
            private bool disposedValue;

            public bool IsActive { get; private set; }
            public int ThreadId { get; private set; }
            private readonly Action _disposeCallback;

            public Scope(int threadId, Action disposeCallback)
            {
                ThreadId = threadId;
                IsActive = true;
                _disposeCallback = disposeCallback;
            }

            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        ThreadId = 0;
                    }

                    disposedValue = true;
                    IsActive = false;
                    _disposeCallback?.Invoke();
                }
            }

            public void Dispose()
            {
                // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }
    }
}
