
using MySharpChat.Core.Utils;
using System;

namespace MySharpChat.Core.Command
{
    public class QuitCommand : Singleton<QuitCommand>, IAsyncMachineCommand
    {
        protected QuitCommand() { }

        public string Name => "Quit";

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
            catch(Exception e)
            {
                throw new CommandException(string.Format("Fail to stop {0}", asyncMachine.ToString()), e);
            }
            return true;
        }

        public string GetHelp()
        {
            return "usage: quit";
        }

        public string GetSummary()
        {
            return "Command to exit the program";
        }
    }
}
