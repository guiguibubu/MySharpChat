using MySharpChat.Core.Utils;
using System.Net;

namespace MySharpChat.Server.Utils
{
    public interface IServerNetworkModule : INetworkModule<HttpListenerContext>
    {
    }
}
