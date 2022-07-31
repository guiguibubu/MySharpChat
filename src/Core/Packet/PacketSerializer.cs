using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace MySharpChat.Core.Packet
{
    public static class PacketSerializer
    {
        public static string Serialize(PacketWrapper? packet)
        {
            if(packet == null)
                throw new ArgumentNullException(nameof(packet));

            return JsonSerializer.Serialize(packet);
        }

        public static PacketWrapper Deserialize(string? data)
        {
            if(string.IsNullOrEmpty(data))
                throw new ArgumentNullException(nameof(data));

            PacketWrapper packet = JsonSerializer.Deserialize<PacketWrapper>(data) ?? throw new NotSupportedException();
            packet.Package = ((JsonElement)packet.Package).Deserialize(Type.GetType(packet.Type)) ?? throw new NotSupportedException();
            return packet;
        }
    }
}
