using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MySharpChat.Core.Http
{
    public interface IHttpRequestHandler
    {
        public void HandleHttpRequest(HttpListenerContext httpContext);
    }
}
