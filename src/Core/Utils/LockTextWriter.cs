using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySharpChat.Core.Utils
{
    public abstract class LockTextWriter : TextWriter, ILockable
    {
        public abstract bool IsLocked { get; }

        public abstract IDisposable Lock();
    }
}
