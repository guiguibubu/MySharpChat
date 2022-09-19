﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace MySharpChat.Core.Packet
{
    public static class PacketSerializer
    {
        public static string Serialize(IEnumerable<PacketWrapper?> packets)
        {
            IEnumerator<PacketWrapper?> enumerator = packets.GetEnumerator();
            StringBuilder sb = new StringBuilder();
            while (enumerator.MoveNext())
            {
                sb.Append(Serialize(enumerator.Current));
            }

            return sb.ToString();
        }

        public static string Serialize(PacketWrapper? packet)
        {
            if (packet == null)
                throw new ArgumentNullException(nameof(packet));

            return JsonSerializer.Serialize(packet);
        }

        public static IEnumerable<PacketWrapper> Deserialize(string? data)
        {
            if (string.IsNullOrEmpty(data))
                throw new ArgumentNullException(nameof(data));
            try
            {
                return DeserializeImpl(data);
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
                listPackets = new List<PacketWrapper>();
                return false;
            }
        }
       
        private static IEnumerable<PacketWrapper> DeserializeImpl(string data)
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
                PacketWrapper packet = JsonSerializer.Deserialize<PacketWrapper>(stringObject) ?? throw new NotSupportedException();
                packet.Package = ((JsonElement)packet.Package).Deserialize(Type.GetType(packet.Type)!) ?? throw new NotSupportedException();
                yield return packet;
            }
        }
    }
}
