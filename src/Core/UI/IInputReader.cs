using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySharpChat.Core.UI
{
    public interface IInputReader
    {
        ConsoleKeyInfo ReadKey(bool intercept);
    }
}
