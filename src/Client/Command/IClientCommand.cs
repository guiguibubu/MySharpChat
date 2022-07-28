using MySharpChat.Core.Command;

namespace MySharpChat.Client.Command
{
    public interface IClientCommand : ICommand<IClientImpl>
    { }
}
