using MySharpChat.Core.Command;

namespace MySharpChat.Client.Command
{
    internal interface IClientCommand : ICommand<IClientImpl>
    { }
}
