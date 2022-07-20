using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySharpChat.Client.Input
{
    internal interface IUserInputCursorContext
    {
        int Width { get; }
        int X { get; set; }
        int Y { get; set; }
    }
}
