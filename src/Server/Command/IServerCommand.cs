using MySharpChat.Core.Command;

namespace MySharpChat.Server.Command
{
    internal interface IServerCommand : ICommand<IServerImpl>
    {
    }
}
