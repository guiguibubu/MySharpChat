using System;

namespace MySharpChat.Core.Utils
{
    public interface ILockable
    {
        IDisposable Lock();
        bool IsLocked { get; }
    }
}
