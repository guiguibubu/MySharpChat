using MySharpChat.Core.Command;

namespace MySharpChat.Client.Console
{
    public interface IClientLogic
    {
        CommandParser CommandParser { get; }
        string Prefix { get; }
    }
}
