using System;
using System.Net.Http;

namespace MySharpChat.Core.Http
{
    public class HttpReadRequestContext : HttpRequestContext
    {
        private HttpReadRequestContext(Uri uri, HttpMethod httpMethod) : base(uri, httpMethod)
        {
        }

        public static HttpReadRequestContext Get(Uri uri) { return new HttpReadRequestContext(uri, HttpMethod.Get); }
    }
}
