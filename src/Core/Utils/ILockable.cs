using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySharpChat.Core.Utils
{
    public interface ILockable
    {
        IDisposable Lock();
        bool IsLocked { get; }
    }
}
