using MySharpChat.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySharpChat.Core.Command
{
    public class HelpCommand : Singleton<HelpCommand>, ICommand
    {
        protected HelpCommand() { }

        public string Name { get => "Help"; }

        public bool Execute(IAsyncMachine? asyncMachine, params string[] args)
        {
            foreach(ICommand command in CommandManager.Instance!.GetCommands())
            {
                Console.WriteLine("{0} :", command.Name);
                string helpMsg;
                try
                {
                    helpMsg = command.GetHelp();
                }
                catch(NotImplementedException)
                {
                    helpMsg = "No help for this command";
                }
                Console.WriteLine(helpMsg);
            }
            return true;
        }

        public bool Execute()
        {
            return Execute(null);
        }

        public string GetHelp()
        {
            throw new NotImplementedException();
        }
    }
}
