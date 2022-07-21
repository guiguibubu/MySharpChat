using System.Reflection;
using MySharpChat.Core.Utils;

namespace MySharpChat.Test
{
    public class SingletonTest
    {
        private List<Type> singletonTypes = new List<Type>();

        [SetUp]
        public void Setup()
        {
            Type singletonType = typeof(Singleton<>);
            Type singletonTypeGeneric = singletonType.GetGenericTypeDefinition();

            Type[] allTypes = Assembly.GetAssembly(singletonTypeGeneric)!.GetTypes();
            singletonTypes = allTypes.Where(t => t.IsClass && !t.IsAbstract && IsSingleton(t)).ToList();
        }

        [Test]
        public void TestValideConstructor()
        {
            TestDelegate testDelegate = () => { };
            foreach(Type type in singletonTypes)
            {
                IEnumerable<MethodInfo> possibleMethods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                            .Where(m => m.Name == Singleton<object>.INSTANCE_CREATOR_NAME && m.GetParameters().Length == 0 && m.ReturnType == type);
                ConstructorInfo[] possibleConstructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                IEnumerable<ConstructorInfo> publicConstructors = possibleConstructors.Where(c => c.IsPublic);
                ConstructorInfo? defaultConstructor = possibleConstructors.FirstOrDefault(c => c.GetParameters().Length == 0);

                testDelegate += () => { Assert.IsFalse(publicConstructors.Any(), "{0} must not have public constructors (currently : {1})", type.FullName, publicConstructors.Count()); };
                testDelegate += () => { Assert.IsTrue(possibleMethods.Count() == 1 || defaultConstructor != null, "{0} must have a default constructor (currently : {2}) or one, and only one, static method \"{0} {1}()\" (currently : {3})", type.FullName, Singleton<object>.INSTANCE_CREATOR_NAME, defaultConstructor?.ToString() ?? "null", possibleMethods.Count()); };
            }
            Assert.Multiple(testDelegate);
        }

        private bool IsSingleton(Type child)
        {
            Type singletonType = typeof(Singleton<>);
            Type singletonTypeGeneric = singletonType.GetGenericTypeDefinition();

            var currentChild = child.IsGenericType
                                   ? child.GetGenericTypeDefinition()
                                   : child;

            while (currentChild != typeof(object))
            {
                if (singletonTypeGeneric == currentChild)
                    return true;

                currentChild = currentChild.BaseType != null
                               && currentChild.BaseType.IsGenericType
                                   ? currentChild.BaseType.GetGenericTypeDefinition()
                                   : currentChild.BaseType;

                if (currentChild == null)
                    return false;
            }
            return false;
        }
    }
}
