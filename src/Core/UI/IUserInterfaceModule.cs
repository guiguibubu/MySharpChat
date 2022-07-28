using MySharpChat.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySharpChat.Core.UI
{
    public interface IUserInterfaceModule
    {
        IUserInputCursorHandler CursorHandler { get; }
        IInputReader InputReader { get; }
        LockTextWriter OutputWriter { get; }
    }
}
