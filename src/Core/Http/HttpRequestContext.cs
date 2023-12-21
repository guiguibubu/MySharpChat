using System;
using System.Net.Http;

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
