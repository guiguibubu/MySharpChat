using System;
using System.IO;

namespace MySharpChat.Core.Utils
{
    public abstract class LockTextWriterAbstract : TextWriter, ILockable
    {
        public abstract bool IsLocked { get; }

        public abstract IDisposable Lock();
    }
}
