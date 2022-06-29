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
        public void Stop();
        public void Wait();
        public bool Wait(int millisecondsTimeout);
    }
}
