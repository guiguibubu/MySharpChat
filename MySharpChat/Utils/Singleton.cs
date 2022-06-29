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
        private static T? _instance = null;
        private static Type _myType = typeof(T);
        public const string INSTANCE_CREATOR_NAME = "GetInstanceImp";

        // We now have a lock object that will be used to synchronize threads
        // during first access to the Singleton.
#pragma warning disable S2743 // Static fields should not be used in generic types
        private static readonly object _lock = new object();
#pragma warning restore S2743 // Static fields should not be used in generic types

        private static T GetInstance()
        {
            // This conditional is needed to prevent threads stumbling over the
            // lock once the instance is ready.
            if (_instance == null)
            {
                // Now, imagine that the program has just been launched. Since
                // there's no Singleton instance yet, multiple threads can
                // simultaneously pass the previous conditional and reach this
                // point almost at the same time. The first of them will acquire
                // lock and will proceed further, while the rest will wait here.
                lock (_lock)
                {
                    // The first thread to acquire the lock, reaches this
                    // conditional, goes inside and creates the Singleton
                    // instance. Once it leaves the lock block, a thread that
                    // might have been waiting for the lock release may then
                    // enter this section. But since the Singleton field is
                    // already initialized, the thread won't create a new
                    // object.
                    if (_instance == null)
                    {
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
                                _instance = (T)possibleConstructor.Invoke(null);
                        }
                        if(_instance == null)
                        {
                            if (nbPossibleMethods == 1)
                            {
#pragma warning disable CS8600 // Conversion de littéral ayant une valeur null ou d'une éventuelle valeur null en type non-nullable.
#pragma warning disable CS8601 // Existence possible d'une assignation de référence null.
                                _instance = (T)possibleMethods.First().Invoke(null, null);
#pragma warning restore CS8601 // Existence possible d'une assignation de référence null.
#pragma warning restore CS8600 // Conversion de littéral ayant une valeur null ou d'une éventuelle valeur null en type non-nullable.
                            }
                            else
                            {
                                throw new NotImplementedException($"{_myType.FullName} must have a default constructor or one, and only one, static method \"{_myType.FullName} {INSTANCE_CREATOR_NAME}()\"");
                            }
                        }
                    }
                }
            }
#pragma warning disable CS8603 // Existence possible d'un retour de référence null.
            return _instance;
#pragma warning restore CS8603 // Existence possible d'un retour de référence null.
        }

        public static T Instance { get { return GetInstance(); } }
    }
}
