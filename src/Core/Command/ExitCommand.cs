namespace MySharpChat.Core.Command
{
    public class ExitCommand : CommandAlias<ExitCommand, QuitCommand>
    {
        protected ExitCommand() { }

        public override string Name { get => "Exit"; }
    }
}
