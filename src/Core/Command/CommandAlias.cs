using System;
using System.Reflection;

using MySharpChat.Core.Utils;

namespace MySharpChat.Core.Command
{
    public abstract class CommandAlias<U, V> : Singleton<U>, IAsyncMachineCommand
        where U : class 
        where V : Singleton<V>, IAsyncMachineCommand
    {
        public abstract string Name { get; }
        private static V parentCommandInstance = GetInstance<V>()!;

        public bool Execute(IAsyncMachine? asyncMachine, params string[] args)
        {
            return parentCommandInstance.Execute(asyncMachine, args);
        }

        public bool Execute(object? data, params string[] args)
        {
            return (this as IAsyncMachineCommand).Execute(data, args);
        }

        public string GetHelp()
        {
            return string.Format("Type \"help {0}\" for more info", parentCommandInstance.Name.ToLowerInvariant());
        }

        public string GetSummary()
        {
            return string.Format("Alias for the commane \"{0}\"", parentCommandInstance.Name.ToLowerInvariant());
        }

        private static T? GetInstance<T>() where T : class
        {
            Type type = typeof(T);
            PropertyInfo? instanceProperty = type.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            if (instanceProperty == null)
                throw new ArgumentException($"The type {type.FullName} must be a {typeof(Singleton<T>).FullName}");
            return (T?)instanceProperty.GetValue(null);
        }
    }
}
