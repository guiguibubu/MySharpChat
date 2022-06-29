using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using MySharpChat.Core.Utils;

namespace MySharpChat.Core.Command
{
    public class ConnectCommand : Singleton<ConnectCommand>, ICommand
    {
        protected ConnectCommand() { }

        public string Name { get => "Connect"; }

        public bool Execute(IAsyncMachine? asyncMachine, params string[] args)
        {
            throw new NotImplementedException();
        }

        public string GetHelp()
        {
            throw new NotImplementedException();
        }
    }
}
