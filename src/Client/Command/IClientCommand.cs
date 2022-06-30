using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            if (asyncMachine is AsynchronousClient client)
            {
                return Execute(client, args);
            }
            else
            {
                throw new ArgumentException(string.Format("{0} must be a {1}", nameof(asyncMachine), typeof(AsynchronousClient)));
            }
        }

        bool Execute(AsynchronousClient client, params string[] args);
    }
}
