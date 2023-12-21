using System;

namespace MySharpChat.Core.Command
{
    public interface ICommand
    {
        string Name { get; }
        bool Execute(object? data, params string[] args);
        string GetSummary();
        string GetHelp();
    }

    public interface ICommand<in T> : ICommand where T : class
    {
        bool Execute(T? data, params string[] args);
        bool ICommand.Execute(object? data, params string[] args)
        {
            if (data is T dataCast)
            {
                return Execute(dataCast, args);
            }
            else
            {
                throw new ArgumentException(string.Format("{0} must be of type {1}", nameof(data), typeof(T).FullName));
            }
        }
    }
}
