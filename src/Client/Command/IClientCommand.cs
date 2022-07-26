using System;

using MySharpChat.Core.Command;
using MySharpChat.Core.Utils;

namespace MySharpChat.Client.Command
{
    internal interface IClientCommand : ICommand
    {
        bool ICommand.Execute(IAsyncMachine? asyncMachine, params string[] args)
        {
            if (asyncMachine == null)
                throw new ArgumentNullException(nameof(asyncMachine));

            if (asyncMachine is IClientImpl client)
            {
                return Execute(client, args);
            }
            else
            {
                throw new ArgumentException(string.Format("{0} must be a {1}", nameof(asyncMachine), typeof(Client)));
            }
        }

        bool Execute(IClientImpl client, params string[] args);
    }
}
