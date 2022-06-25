using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;

namespace MySharpChat
{
    /// <summary>
    /// Inspiration from https://github.com/rgregg/markdown-scanner/blob/master/ApiDocs.Validation/Http/HttpParser.cs
    /// </summary>
    public static partial class HttpParser
    {
        public const string DEFAULT_HTTP_VERSION = "1.1";
        /// <summary>
        /// Converts a raw HTTP request into an System.Net.Http.HttpRequestMessage instance.
        /// </summary>
        /// <param name="requestString"></param>
        /// <returns>System.Net.Http.HttpRequestMessage</returns>
        public static HttpRequestMessage ParseHttpRequest(string requestString)
        {
            return HttpRequestParser.Parse(requestString);
        }

        public static bool TryParseHttpRequest(string requestString, out HttpRequestMessage httpRequestMessage)
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

        /// <summary>
        /// Convert a raw HTTP response into an System.Net.Http.HttpResponseMessage instance.
        /// </summary>
        /// <param name="responseString"></param>
        /// <returns>System.Net.Http.HttpResponseMessage</returns>
        /*
        public static HttpResponseMessage ParseHttpResponse(string responseString)
        {
            StringReader reader = new StringReader(responseString);
            string line;
            ParserState mode = ParserState.FirstLine;

            HttpResponseMessage response = new HttpResponseMessage();

            while ((line = reader.ReadLine()) != null)
            {
                switch (mode)
                {
                    case ParserState.FirstLine:
                        var components = line.Split(' ');
                        if (components.Length < 3) throw new ArgumentException("responseString does not contain a proper HTTP request first line.");

                        response.HttpVersion = components[0];
                        response.StatusCode = int.Parse(components[1]);
                        response.StatusMessage = components.ComponentsJoinedByString(" ", 2);

                        mode = ParserState.Headers;
                        break;

                    case ParserState.Headers:
                        if (string.IsNullOrEmpty(line))
                        {
                            mode = ParserState.Content;
                            continue;
                        }

                        // Parse each header
                        int split = line.IndexOf(": ", StringComparison.Ordinal);
                        if (split < 1) throw new ArgumentException($"Request contains an invalid header definition: \"{line}\". Missing whitespace between the headers and body?");

                        var headerName = line.Substring(0, split);
                        var headerValue = line.Substring(split + 1);
                        response.Headers.Add(headerName, headerValue);

                        break;

                    case ParserState.Content:
                        response.Body = line + Environment.NewLine + reader.ReadToEnd();
                        break;
                }
            }

            return response;
        }
        */

        private enum ParserState
        {
            FirstLine,
            Headers,
            Content
        }
    }
}
