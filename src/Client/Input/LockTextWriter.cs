using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySharpChat.Client.Input
{
    internal abstract class LockTextWriter : TextWriter
    {
        public abstract IDisposable Lock();
        public abstract bool IsLocked { get; }
    }
}
