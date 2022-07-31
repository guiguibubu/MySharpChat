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
        public PacketWrapper(Guid sourceId, object package)
        {
            SourceId = sourceId;
            Type = package.GetType().AssemblyQualifiedName!;
            Package = package;
        }

        public Guid SourceId { get; set; }
        public string Type { get; set; }
        public object Package { get; set; }
    }
}
