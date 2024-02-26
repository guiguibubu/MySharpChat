using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace MySharpChat.Core.Packet
{
    public static class PacketSerializer
    {
        public static string Serialize<T>(IEnumerable<T?> packets)
        {
            StringBuilder sb = new();
            foreach (T? packet in packets)
            {
                sb.Append(Serialize(packet));
            }

            return sb.ToString();
        }

        public static string Serialize<T>(T? packet)
        {
            ArgumentNullException.ThrowIfNull(packet);

            return JsonSerializer.Serialize(packet);
        }

        public static IEnumerable<PacketWrapper> Deserialize(string? data)
        {
            ArgumentNullException.ThrowIfNull(data);

            try
            {
                return DeserializeImpl(data);
            }
            catch (JsonException e)
            {
                throw new ArgumentException("Fail deserialize this : " + data, e);
            }
        }

        public static IEnumerable<PacketWrapper<T>> Deserialize<T>(string? data)
        {
            ArgumentNullException.ThrowIfNull(data);

            try
            {
                return DeserializeImpl<T>(data);
            }
            catch (JsonException e)
            {
                throw new ArgumentException("Fail deserialize this : " + data, e);
            }
        }

        public static bool TryDeserialize(string? data, out IEnumerable<PacketWrapper> listPackets)
        {
            try
            {
                listPackets = Deserialize(data);
                return true;
            }
            catch
            {
                listPackets = Array.Empty<PacketWrapper>();
                return false;
            }
        }

        public static bool TryDeserialize<T>(string? data, out IEnumerable<PacketWrapper<T>> listPackets)
        {
            try
            {
                listPackets = Deserialize<T>(data);
                return true;
            }
            catch
            {
                listPackets = Array.Empty<PacketWrapper<T>>();
                return false;
            }
        }

        private static IEnumerable<PacketWrapper> DeserializeImpl(string data)
        {
            IEnumerable<PacketWrapper> packets = DeserializeObjectsImpl<PacketWrapper>(data);
            foreach (PacketWrapper packet in packets)
            {
                object package = ((JsonElement)packet.Package).Deserialize(Type.GetType(packet.Type)!) ?? throw new NotSupportedException();
                yield return new PacketWrapper(packet.SourceId, package);
            }
        }

        private static IEnumerable<PacketWrapper<T>> DeserializeImpl<T>(string data)
        {
            IEnumerable<PacketWrapper> packets = DeserializeImpl(data);
            return packets.Select(p => new PacketWrapper<T>(p.SourceId, (T)p.Package));
        }

        private static IEnumerable<T> DeserializeObjectsImpl<T>(string data)
        {
            List<byte> objectBytes;
            List<byte> remainingBytes = Encoding.UTF8.GetBytes(data).ToList();
            while (remainingBytes.Any())
            {
                Utf8JsonReader reader = new Utf8JsonReader(new ReadOnlySpan<byte>(remainingBytes.ToArray()));
                bool endOfObject = false;
                int objectCount = 0;
                while (!endOfObject && reader.Read())
                {
                    JsonTokenType tokenType = reader.TokenType;
                    switch (tokenType)
                    {
                        case JsonTokenType.StartObject:
                            objectCount++;
                            break;
                        case JsonTokenType.EndObject:
                            objectCount--;
                            break;
                        case JsonTokenType.None:
                        case JsonTokenType.StartArray:
                        case JsonTokenType.EndArray:
                        case JsonTokenType.PropertyName:
                        case JsonTokenType.Comment:
                        case JsonTokenType.String:
                        case JsonTokenType.Number:
                        case JsonTokenType.True:
                        case JsonTokenType.False:
                        case JsonTokenType.Null:
                            break;
                    }

                    endOfObject = objectCount == 0;
                }

                int nbBytes = (int)reader.BytesConsumed;
                objectBytes = remainingBytes.GetRange(0, nbBytes);
                remainingBytes = remainingBytes.GetRange(nbBytes, remainingBytes.Count - nbBytes);

                string stringObject = Encoding.UTF8.GetString(objectBytes.ToArray());
                //DeserializeAsyncEnumerable
                T packet = JsonSerializer.Deserialize<T>(stringObject) ?? throw new NotSupportedException();
                yield return packet;
            }
        }
    }
}
