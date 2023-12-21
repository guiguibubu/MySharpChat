﻿using System;
using System.Net.Http;

namespace MySharpChat.Core.Http
{
    public class HttpSendRequestContext : HttpRequestContext
    {
        private HttpSendRequestContext(Uri uri, HttpMethod httpMethod) : base(uri, httpMethod)
        {
        }

        public static HttpSendRequestContext Post(Uri uri) { return new HttpSendRequestContext(uri, HttpMethod.Post); }
        public static HttpSendRequestContext Put(Uri uri) { return new HttpSendRequestContext(uri, HttpMethod.Put); }
        public static HttpSendRequestContext Delete(Uri uri) { return new HttpSendRequestContext(uri, HttpMethod.Delete); }
    }
}
