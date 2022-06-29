using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using MySharpChat.Core.Command;
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

#pragma warning disable CS8602 // Déréférencement d'une éventuelle référence null.
            Type[] allTypes = Assembly.GetAssembly(singletonTypeGeneric).GetTypes();
            singletonTypes = allTypes.Where(t => t.IsClass && !t.IsAbstract && IsSingleton(t)).ToList();
#pragma warning restore CS8602 // Déréférencement d'une éventuelle référence null.
        }

        [Test]
        public void TestValideConstructor()
        {
            TestDelegate testDelegate = () => { };
            foreach(Type type in singletonTypes)
            {
                TestContext.Out.WriteLine(type.FullName);
                IEnumerable<MethodInfo> possibleMethods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
                            .Where(m => m.Name == Singleton<object>.INSTANCE_CREATOR_NAME && m.GetParameters().Length == 0 && m.ReturnType == type);
                ConstructorInfo? possibleConstructor = type.GetConstructor(BindingFlags.NonPublic, System.Type.EmptyTypes);

                testDelegate += () => { Assert.IsTrue(possibleMethods.Count() == 1 || possibleConstructor != null, "{0} must have a default constructor or one, and only one, static method \"{0} {1}()\"", type.FullName, Singleton<object>.INSTANCE_CREATOR_NAME); };
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
