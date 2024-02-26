using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MySharpChat.Core.Utils
{
    public class SafeThread
    {
        private readonly Thread _thread;
        private Exception? _exception;

        public SafeThread(ParameterizedThreadStart start)
        {
            _thread = new Thread((object? obj) => SafeExecute(start, obj));
        }
        public SafeThread(ThreadStart start)
        {
            _thread = new Thread(() => SafeExecute(start));
        }
        public SafeThread(ParameterizedThreadStart start, int maxStackSize)
        {
            _thread = new Thread((object? obj) => SafeExecute(start, obj), maxStackSize);
        }
        public SafeThread(ThreadStart start, int maxStackSize)
        {
            _thread = new Thread(() => SafeExecute(start), maxStackSize);
        }

        public void Start()
        {
            _thread.Start();
        }

        public void Start(object? parameter)
        {
            _thread.Start(parameter);
        }

        public void Join()
        {
            while (!_thread.Join(TimeSpan.FromSeconds(1)))
            {
                ThrowIfException();
            }
        }

        public bool Join(int millisecondsTimeout)
        {
            bool result = _thread.Join(millisecondsTimeout);
            ThrowIfException();
            return result;
        }

        public bool Join(TimeSpan timeout)
        {
            bool result = _thread.Join(timeout);
            ThrowIfException();
            return result;
        }

        private void ThrowIfException()
        {
            if (_exception is not null)
                throw new AggregateException(_exception);
        }

        private void SafeExecute(ThreadStart action)
        {
            try
            {
                action.Invoke();
            }
            catch (Exception ex)
            {
                _exception = ex;
                throw;
            }
        }

        private void SafeExecute(ParameterizedThreadStart action, object? obj)
        {
            try
            {
                action.Invoke(obj);
            }
            catch (Exception ex)
            {
                _exception = ex;
                throw;
            }
        }
    }
}
