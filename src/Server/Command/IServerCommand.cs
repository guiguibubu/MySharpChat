using System;

using MySharpChat.Core.Command;
using MySharpChat.Core.Utils;

namespace MySharpChat.Server.Command
{
    internal interface IServerCommand : ICommand<IServerImpl>
    {
    }
}
