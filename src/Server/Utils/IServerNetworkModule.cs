using System.Net;
using MySharpChat.Core.Utils;

namespace MySharpChat.Server.Utils
{
    public interface IServerNetworkModule : INetworkModule<HttpListenerContext>
    {
    }
}
