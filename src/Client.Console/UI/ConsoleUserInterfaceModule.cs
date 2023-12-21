using MySharpChat.Core.Console;

namespace MySharpChat.Client.Console.UI
{
    internal class ConsoleUserInterfaceModule : IUserInterfaceModule
    {
        public ConsoleUserInterfaceModule(ConsoleInputReader inputReader, ConsoleOutputModule output)
        {
            InputModule = inputReader;
            OutputModule = output;
        }

        public ConsoleUserInterfaceModule() : this(new ConsoleInputReader(), new ConsoleOutputModule()) { }

        public ConsoleInputReader InputModule { get; }

        public ConsoleOutputModule OutputModule { get; private set; }
    }
}
