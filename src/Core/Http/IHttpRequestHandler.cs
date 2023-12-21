using System.Net;

namespace MySharpChat.Core.Http
{
    public interface IHttpRequestHandler
    {
        public void HandleHttpRequest(HttpListenerContext httpContext);
    }
}
