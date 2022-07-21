using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySharpChat.Core.SocketModule;
using MySharpChat.Core.Utils;

namespace MySharpChat.Core.Command
{
    public class SendCommand : Singleton<SendCommand>, ICommand
    {
        protected SendCommand() { }

        public string Name => "Send";

        public bool Execute(IAsyncMachine? asyncMachine, params string[] args)
        {
            if (asyncMachine == null)
                throw new ArgumentNullException(nameof(asyncMachine));

            string? text = args.Length > 0 ? args[0] : null;
            asyncMachine.Send(text);

            return true;
        }

        public string GetHelp()
        {
            return "usage: send <text>";
        }

        public string GetSummary()
        {
            return "Command to send messages";
        }
    }
}
