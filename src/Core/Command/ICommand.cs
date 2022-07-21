using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MySharpChat.Core.Utils;

namespace MySharpChat.Core.Command
{
    public interface ICommand
    {
        string Name { get; }
        bool Execute(IAsyncMachine? asyncMachine, params string[] args);
        string GetSummary();
        string GetHelp();
    }
}
