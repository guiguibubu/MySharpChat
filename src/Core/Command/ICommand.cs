
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
