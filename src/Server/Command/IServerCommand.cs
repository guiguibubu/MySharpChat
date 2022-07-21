using System;

using MySharpChat.Core.Command;
using MySharpChat.Core.Utils;

namespace MySharpChat.Server.Command
{
    internal interface IServerCommand : ICommand
    {
        bool ICommand.Execute(IAsyncMachine? asyncMachine, params string[] args)
        {
            if (asyncMachine == null)
                throw new ArgumentNullException(nameof(asyncMachine));

            if (asyncMachine is AsynchronousServer server)
            {
                return Execute(server, args);
            }
            else
            {
                throw new ArgumentException(string.Format("{0} must be a {1}", nameof(asyncMachine), typeof(AsynchronousServer)));
            }
        }

        bool Execute(AsynchronousServer server, params string[] args);
    }
}
