using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace MySharpChat.Core.Http
{
    public static partial class HttpParser
    {
        private static class HttpRequestParser
        {
            /// <summary>
            /// Converts a raw HTTP request into an System.Net.Http.HttpRequestMessage instance.
            /// </summary>
            /// <param name="requestString"></param>
            /// <returns>System.Net.Http.HttpRequestMessage</returns>
            public static HttpRequestMessage Parse(string requestString)
            {
                if (string.IsNullOrWhiteSpace(requestString))
                {
                    throw new ArgumentException("requestString was empty or whitespace only. Not a valid HTTP request.");
                }

                StringReader reader = new StringReader(requestString);
                string? line;
                ParserState state = ParserState.FirstLine;

                HttpRequestMessage request = new HttpRequestMessage();

                while ((line = reader.ReadLine()) != null)
                {
                    switch (state)
                    {
                        case ParserState.FirstLine:
                            var components = line.Trim().Split(' ');
                            if (components.Length < 2)
                                throw new FormatException("RequestString does not contain a proper HTTP request first line (not enough infos).");
                            if (components.Length > 3)
                                throw new FormatException("RequestString does not contain a proper HTTP request first line (more than the expected 3 components)");

                            if (components[0].StartsWith("HTTP/"))
                                throw new FormatException("RequestString contains an HTTP response.");

                            request.Method = new HttpMethod(components[0]);
                            request.RequestUri = new Uri(components[1], UriKind.Relative);

                            if (components.Length >= 3)
                            {
                                string version = components[2];
                                if (TryParseHttpVersion(version, out Version? requestVersion))
                                {
#pragma warning disable CS8601 // Existence possible d'une assignation de référence null.
                                    request.Version = requestVersion;
#pragma warning restore CS8601 // Existence possible d'une assignation de référence null.
                                }
                                else
                                {
                                    throw new FormatException("RequestString does not contain a proper HTTP request first line (wrong format for HTTP version)");
                                }
                            }
                            else
                            {
                                request.Version = Version.Parse(DEFAULT_HTTP_VERSION);
                            }

                            state = ParserState.Headers;
                            break;

                        case ParserState.Headers:
                            if (string.IsNullOrEmpty(line))
                            {
                                state = ParserState.Content;
                                continue;
                            }

                            // Parse each header
                            int split = line.IndexOf(": ", StringComparison.Ordinal);
                            if (split < 1)
                                throw new FormatException("requestString contains an invalid header definition");

                            string headerName = line.Substring(0, split);
                            string headerValue = line.Substring(split + 1);
                            request.Headers.Add(headerName, headerValue);

                            break;

                        case ParserState.Content:
                            string restOfBody = reader.ReadToEnd() ?? string.Empty;

                            // normalize line endings to CRLF, which is required for headers, etc.
                            restOfBody = restOfBody.Replace("\r\n", "\n").Replace("\n", "\r\n");
                            request.Content = new StringContent(string.Concat(line, "\r\n", restOfBody));

                            break;
                    }
                }

                return request;
            }

            public static async Task<string> ToString(HttpRequestMessage request)
            {
                StringBuilder sb = new StringBuilder();

                string line1 = $"{request.Method} {request.RequestUri} HTTP/{request.Version}";
                sb.AppendLine(line1);

                foreach ((string key, IEnumerable<string> values) in request.Headers)
                    foreach (var value in values)
                    {
                        string header = $"{key}: {value}";
                        sb.AppendLine(header);
                    }

                if (request.Content?.Headers != null)
                {
                    foreach ((string key, IEnumerable<string> values) in request.Content.Headers)
                        foreach (var valus in values)
                        {
                            string header = $"{key}: {valus}";
                            sb.AppendLine(header);
                        }
                }
                sb.AppendLine();

                string body = await (request.Content?.ReadAsStringAsync() ?? Task.FromResult(""));
                if (!string.IsNullOrWhiteSpace(body))
                    sb.AppendLine(body);

                return sb.ToString();
            }
        }
    }
}
