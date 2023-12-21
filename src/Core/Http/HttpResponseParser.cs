using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MySharpChat.Core.Http
{
    public static partial class HttpParser
    {
        private static class HttpResponseParser
        {
            /// <summary>
            /// Convert a raw HTTP response into an System.Net.Http.HttpResponseMessage instance.
            /// </summary>
            /// <param name="responseString"></param>
            /// <returns>System.Net.Http.HttpResponseMessage</returns>
            public static HttpResponseMessage ParseHttpResponse(string responseString)
            {
                StringReader reader = new StringReader(responseString);
                string? line;
                ParserState mode = ParserState.FirstLine;

                HttpResponseMessage response = new HttpResponseMessage();

                while ((line = reader.ReadLine()) != null)
                {
                    switch (mode)
                    {
                        case ParserState.FirstLine:
                            var components = line.Split(' ');
                            if (components.Length < 3) throw new ArgumentException("responseString does not contain a proper HTTP request first line.");

                            string version = components[0];
                            if (TryParseHttpVersion(version, out Version? responseVersion))
                            {
                                response.Version = responseVersion!;
                            }
                            else
                            {
                                throw new FormatException("ResponseString does not contain a proper HTTP request first line (wrong format for HTTP version)");
                            }

                            response.StatusCode = Enum.Parse<HttpStatusCode>(components[1]);
                            int offset = 2;
                            response.ReasonPhrase = string.Join(" ", new ArraySegment<string>(components, offset, components.Length - offset));

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
                            if (split < 1)
                                throw new ArgumentException($"Response contains an invalid header definition: \"{line}\". Missing whitespace between the headers and body?");

                            var headerName = line.Substring(0, split);
                            var headerValue = line.Substring(split + 1);
                            response.Headers.Add(headerName, headerValue);

                            break;

                        case ParserState.Content:

                            string restOfBody = reader.ReadToEnd() ?? string.Empty;

                            // normalize line endings to CRLF, which is required for headers, etc.
                            restOfBody = restOfBody.Replace("\r\n", "\n").Replace("\n", "\r\n");
                            response.Content = new StringContent(string.Concat(line, "\r\n", restOfBody));

                            break;
                    }
                }

                return response;
            }

            public static async Task<string> ToString(HttpResponseMessage response)
            {
                var sb = new StringBuilder();

                int statusCode = (int)response.StatusCode;
                string line1 = $"HTTP/{response.Version} {statusCode} {response.ReasonPhrase}";
                sb.AppendLine(line1);

                foreach ((string key, IEnumerable<string> values) in response.Headers)
                    foreach (string value in values)
                    {
                        string header = $"{key}: {value}";
                        sb.AppendLine(header);
                    }

                foreach ((string key, IEnumerable<string> values) in response.Content.Headers)
                    foreach (string value in values)
                    {
                        string header = $"{key}: {value}";
                        sb.AppendLine(header);
                    }
                sb.AppendLine();

                string body = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrWhiteSpace(body))
                    sb.AppendLine(body);

                return sb.ToString();
            }
        }
    }
}
