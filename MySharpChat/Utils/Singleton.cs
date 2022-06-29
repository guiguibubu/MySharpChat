using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MySharpChat.Core.Utils
{
    public abstract class Singleton<T> where T : class
    {
        private static Lazy<T> _instance = new Lazy<T>(() => GetInstance());
        private static Type _myType = typeof(T);
        public const string INSTANCE_CREATOR_NAME = "GetInstanceImp";

        // We now have a lock object that will be used to synchronize threads
        // during first access to the Singleton.
#pragma warning disable S2743 // Static fields should not be used in generic types
        private static readonly object _lock = new object();
#pragma warning restore S2743 // Static fields should not be used in generic types

        private static T GetInstance()
        {
            T? instance = null;
#pragma warning disable S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields
            IEnumerable<MethodInfo> possibleMethods = _myType.GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                .Where(m => m.Name == INSTANCE_CREATOR_NAME && m.GetParameters().Length == 0 && m.ReturnType == _myType);
#pragma warning restore S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields
            int nbPossibleMethods = possibleMethods.Count();
            if (nbPossibleMethods == 0)
            {
#pragma warning disable S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields
                ConstructorInfo? possibleConstructor = _myType.GetConstructor(BindingFlags.NonPublic, System.Type.EmptyTypes);
#pragma warning restore S3011 // Reflection should not be used to increase accessibility of classes, methods, or fields
                if (possibleConstructor != null)
                    instance = (T)possibleConstructor.Invoke(null);
            }
            if (_instance == null)
            {
                if (nbPossibleMethods == 1)
                {
#pragma warning disable CS8600 // Conversion de littéral ayant une valeur null ou d'une éventuelle valeur null en type non-nullable.
#pragma warning disable CS8601 // Existence possible d'une assignation de référence null.
                    instance = (T)possibleMethods.First().Invoke(null, null);
#pragma warning restore CS8601 // Existence possible d'une assignation de référence null.
#pragma warning restore CS8600 // Conversion de littéral ayant une valeur null ou d'une éventuelle valeur null en type non-nullable.
                }
                else
                {
                    throw new NotImplementedException($"{_myType.FullName} must have a default constructor or one, and only one, static method \"{_myType.FullName} {INSTANCE_CREATOR_NAME}()\"");
                }
            }
#pragma warning disable CS8603 // Existence possible d'un retour de référence null.
            return instance;
#pragma warning restore CS8603 // Existence possible d'un retour de référence null.
        }

        public static T Instance { get { return _instance.Value; } }
    }
}
