using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MySharpChat.Core.Http
{
    public abstract class HttpRequestContext
    {
        public Uri Uri { get; private set; }
        public HttpMethod HttpMethod { get; private set; }

        protected HttpRequestContext(Uri uri, HttpMethod httpMethod)
        {
            Uri = uri;
            HttpMethod = httpMethod;
        }
    }
}
