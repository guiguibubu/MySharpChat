using MySharpChat.Core.Packet;
using MySharpChat.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySharpChat.Client.Utils
{
    public interface IClientNetworkModule : INetworkModule<PacketWrapper>
    {
    }
}
