using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

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
