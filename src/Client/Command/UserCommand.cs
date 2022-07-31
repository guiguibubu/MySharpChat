using MySharpChat.Core.Command;

namespace MySharpChat.Client.Command
{
    public class UserCommand : CommandAlias<UserCommand, UsernameCommand>, IClientCommand
    {
        protected UserCommand() { }

        public override string Name => "User";

        public bool Execute(IClientImpl? data, params string[] args)
        {
            return Execute<IClientImpl>(data, args);
        }
    }
}
