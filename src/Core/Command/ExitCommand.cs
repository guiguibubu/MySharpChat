using MySharpChat.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySharpChat.Core.Command
{
    public class ExitCommand : CommandAlias<ExitCommand, QuitCommand>
    {
        protected ExitCommand() { }

        public override string Name { get => "Exit"; }
    }
}
