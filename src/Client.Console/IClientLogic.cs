using MySharpChat.Core.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySharpChat.Client.Console
{
    public interface IClientLogic
    {
        CommandParser CommandParser { get; }
        string Prefix { get; }
    }
}
