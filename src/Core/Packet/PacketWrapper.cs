using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySharpChat.Core.Packet
{
    [Serializable]
    public class PacketWrapper
    {
        public PacketWrapper(string id, object package)
        {
            Id = id;
            Type = package.GetType().AssemblyQualifiedName!;
            Package = package;
        }

        public string Id { get; set; }
        public string Type { get; set; }
        public object Package { get; set; }
    }
}
