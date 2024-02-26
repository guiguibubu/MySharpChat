using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace MySharpChat.Core.Packet
{
    [Serializable]
    public class PacketWrapper: PacketWrapper<object>
    {
        public PacketWrapper(Guid sourceId, object package) : base(sourceId, package)
        { }

        [JsonConstructor]
        public PacketWrapper(Guid sourceId, string type, object package) : base(sourceId, type, package)
        { }
    }

    [Serializable]
    public class PacketWrapper<T> : IPacketWrapper<T>
    {
        public PacketWrapper(Guid sourceId, [DisallowNull]T package) : this(sourceId, package.GetType().AssemblyQualifiedName!, package)
        { }

        [JsonConstructor]
        public PacketWrapper(Guid sourceId, string type, T package)
        {
            ArgumentNullException.ThrowIfNull(package);
            SourceId = sourceId;
            Type = type;
            Package = package;
        }

        public Guid SourceId { get; }
        public string Type { get; }
        public T Package { get; }
    }

    public interface IPacketWrapper<out T>
    {
        public Guid SourceId { get; }
        public string Type { get; }
        public T Package { get; }
    }
}
