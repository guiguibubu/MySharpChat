using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using MySharpChat.Core.Utils;

namespace MySharpChat.Core.Command
{
    public class QuitCommand : Singleton<QuitCommand>, ICommand
    {
        protected QuitCommand() { }

        public string Name { get => "Quit"; }

        public bool Execute(IAsyncMachine? asyncMachine, params string[] args)
        {
            if(asyncMachine == null)
                return true;

            try
            {
                if(args.Length == 1 && int.TryParse(args[0], out int exitCode))
                {
                    asyncMachine.Stop(exitCode);
                }
                else
                {
                    asyncMachine.Stop();
                }
            }
            catch
            {
                throw new CommandException("Fail to stop {0}", asyncMachine.ToString());
            }
            return true;
        }

        public string GetHelp()
        {
            throw new NotImplementedException();
        }
    }
}
