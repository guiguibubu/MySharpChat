﻿using System;
using System.Reflection;

using MySharpChat.Core.Utils;

namespace MySharpChat.Core.Command
{
    public abstract class CommandAlias<U, V> : Singleton<U>, ICommand
        where U : class 
        where V : Singleton<V>, ICommand
    {
        public abstract string Name { get; }
        private static V parentCommandInstance = GetInstance<V>()!;

        protected bool Execute<T>(T? data, params string[] args) where T : class
        {
            return parentCommandInstance.Execute(data, args);
        }

        public bool Execute(object? data, params string[] args)
        {
            return parentCommandInstance.Execute(data, args);
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
            if (instanceProperty is null)
                throw new ArgumentException($"The type {type.FullName} must be a {typeof(Singleton<T>).FullName}");
            return (T?)instanceProperty.GetValue(null);
        }
    }
}
