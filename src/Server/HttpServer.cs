using MySharpChat.Core.Utils.Logger;
using System;
using System.IO;
using System.Net;

namespace MySharpChat.Server
{
    public class HttpServer
    {
        private static readonly Logger logger = Logger.Factory.GetLogger<HttpServer>();

#if DEBUG
        public const int HTTP_PORT = 8080;
#else
        public const int HTTP_PORT = 80;
#endif
        private readonly HttpListener _listener;

        public HttpServer()
        {
            _listener = new HttpListener();
        }

        public void Start(IPEndPoint endpoint)
        {
            string httpAdresse = string.Format("http://{0}:{1}/", endpoint.Address, HTTP_PORT);
            _listener.Prefixes.Add(httpAdresse);
            try
            {
                _listener.Start();
            }
            catch (HttpListenerException e)
            {
                logger.LogError("{0} catched, Error code associated : {1}", nameof(HttpListenerException), e.ErrorCode);
                throw;
            }
            Receive();
        }

        public void Stop()
        {
            _listener.Stop();
        }

        private void Receive()
        {
            _listener.BeginGetContext(new AsyncCallback(ListenerCallback), _listener);
        }

        private void ListenerCallback(IAsyncResult result)
        {
            if (_listener.IsListening)
            {
                HttpListenerContext context = _listener.EndGetContext(result);
                HttpListenerRequest request = context.Request;

                HttpListenerResponse response = context.Response;
                Stream output = response.OutputStream;

                byte[] bodyBytes;

                //Remove the first '/' character
                string uriPath = request.Url!.AbsolutePath.Substring(1);
                string osPath = !string.IsNullOrEmpty(uriPath) ? uriPath.Replace('/', Path.DirectorySeparatorChar) : "index.html";

                if (File.Exists(Path.Combine("res", osPath)))
                {
                    bodyBytes = File.ReadAllBytes(Path.Combine("res", osPath));
                    response.StatusCode = (int)HttpStatusCode.OK;
                }
                else
                {
                    string text = "Welcome on MySharpChat server.";
                    text += Environment.NewLine;
                    text += $"No data at {request.Url!}";
                    response.StatusCode = (int)HttpStatusCode.NotFound;

                    bodyBytes = System.Text.Encoding.UTF8.GetBytes(text);
                }

                response.ContentLength64 = bodyBytes.Length;
                output.Write(bodyBytes);
                output.Close();
                response.Close();

                Receive();
            }
        }
    }
}
