using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;

namespace MySharpChat.Core.Http
{
    /// <summary>
    /// Inspiration from https://github.com/rgregg/markdown-scanner/blob/master/ApiDocs.Validation/Http/HttpParser.cs
    /// </summary>
    public static partial class HttpParser
    {
        public const string DEFAULT_HTTP_VERSION = "1.1";

        #region Request
        /// <summary>
        /// Converts a raw HTTP request into an System.Net.Http.HttpRequestMessage instance.
        /// </summary>
        /// <param name="requestString"></param>
        /// <returns>System.Net.Http.HttpRequestMessage</returns>
        public static HttpRequestMessage ParseHttpRequest(string requestString)
        {
            return HttpRequestParser.Parse(requestString);
        }

        public static bool TryParseHttpRequest(string requestString, out HttpRequestMessage? httpRequestMessage)
        {
            bool success;
            try
            {
                httpRequestMessage = ParseHttpRequest(requestString);
                success = true;
            }
            catch
            {
                httpRequestMessage = null;
                success = false;
            }
            return success;
        }

        public static async Task<string> ToString(HttpRequestMessage request)
        {
            return await HttpRequestParser.ToString(request);
        }
        #endregion

        #region Response
        /// <summary>
        /// Convert a raw HTTP response into an System.Net.Http.HttpResponseMessage instance.
        /// </summary>
        /// <param name="responseString"></param>
        /// <returns>System.Net.Http.HttpResponseMessage</returns>
        public static HttpResponseMessage ParseHttpResponse(string responseString)
        {
            return HttpResponseParser.ParseHttpResponse(responseString);
        }

        public static bool TryParseHttpResponse(string responseString, out HttpResponseMessage? httpResponsetMessage)
        {
            bool success;
            try
            {
                httpResponsetMessage = ParseHttpResponse(responseString);
                success = true;
            }
            catch
            {
                httpResponsetMessage = null;
                success = false;
            }
            return success;
        }

        private static bool TryParseHttpVersion(string text, out Version? httpVersion)
        {
            httpVersion = null;
            string version = text;
            return version.StartsWith("HTTP/") && Version.TryParse(version.Replace("HTTP/", ""), out httpVersion);
        }

        public static async Task<string> ToString(HttpResponseMessage response)
        {
            return await HttpResponseParser.ToString(response);
        }
#endregion

        private enum ParserState
        {
            FirstLine,
            Headers,
            Content
        }
    }
}
