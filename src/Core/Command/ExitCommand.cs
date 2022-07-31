using MySharpChat.Core.Utils;

namespace MySharpChat.Core.Command
{
    public class ExitCommand : CommandAlias<ExitCommand, QuitCommand>, IAsyncMachineCommand
    {
        protected ExitCommand() { }

        public override string Name => "Exit";

        public bool Execute(IAsyncMachine? data, params string[] args)
        {
            return Execute<IAsyncMachine>(data, args);
        }
    }
}
