using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySharpChat.Core.Command
{
    public interface ICommand
    {
        string Name { get; }
        bool Execute(params string[] args);
        string GetHelp();
    }
}
