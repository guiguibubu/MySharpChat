using System;

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
