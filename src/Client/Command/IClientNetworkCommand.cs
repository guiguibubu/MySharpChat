using System;

using MySharpChat.Core.Command;
using MySharpChat.Core.Utils;

namespace MySharpChat.Client.Command
{
    internal interface IClientNetworkCommand : ICommand<INetworkModule>
    {
    }
}
