using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MySharpChat.Core.Utils
{
    public abstract class Singleton<T> where T : class
    {
        private static Lazy<T?> _instance = new Lazy<T?>(() => GetInstance());
        private static Type _myType = typeof(T);
        public const string INSTANCE_CREATOR_NAME = "GetInstanceImp";

        private static T? GetInstance()
        {
            T? instance = null;
            Type type = _myType;
#pragma warning disable S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields
            IEnumerable<MethodInfo> possibleMethods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                            .Where(m => m.Name == Singleton<object>.INSTANCE_CREATOR_NAME && m.GetParameters().Length == 0 && m.ReturnType == type);
#pragma warning restore S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields
            int nbPossibleMethods = possibleMethods.Count();
            if (nbPossibleMethods == 0)
            {
#pragma warning disable S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields
                ConstructorInfo? possibleConstructor = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, Type.EmptyTypes);
#pragma warning restore S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields
                if (possibleConstructor != null)
                    instance = (T)possibleConstructor.Invoke(null);
            }
            if (_instance == null)
            {
                if (nbPossibleMethods == 1)
                {
                    instance = (T?)possibleMethods.First().Invoke(null, null);
                }
                else
                {
                    throw new NotImplementedException($"{type.FullName} must have a default constructor or one, and only one, static method \"{type.FullName} {INSTANCE_CREATOR_NAME}()\"");
                }
            }
            return instance;
        }

        public static T? Instance { get { return _instance.Value; } }
    }
}
