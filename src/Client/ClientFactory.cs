using MySharpChat.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySharpChat.Client
{
    public partial class Client
    {
        internal static class Factory
        {
            public static Client Initialize(object? initObject = null)
            {
                Client.Instance.Initialize(initObject);
                return Client.Instance;
            }
        }
    }
}
