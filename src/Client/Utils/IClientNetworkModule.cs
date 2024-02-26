using MySharpChat.Core.Event;
using MySharpChat.Core.Packet;
using MySharpChat.Core.Utils;

namespace MySharpChat.Client.Utils
{
    public interface IClientNetworkModule : INetworkModule<PacketWrapper<ChatEvent>>
    {
    }
}
