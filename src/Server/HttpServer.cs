using MySharpChat.Core.Utils.Logger;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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
        public ReadOnlyCollection<string> Prefixes => _listener.Prefixes.ToList().AsReadOnly();
        private bool _running = false;
        public bool IsRunning => _running;

        public readonly Queue<HttpListenerContext> requestQueue = new Queue<HttpListenerContext>();

        public HttpServer()
        {
            _listener = new HttpListener();
        }

        public void Start(IPAddress? ipAdress)
        {
            if (ipAdress == null)
                throw new ArgumentNullException(nameof(ipAdress));

            string httpAdresse = string.Format("http://{0}:{1}/", ipAdress.ToString(), HTTP_PORT);
            _listener.Prefixes.Add(httpAdresse);
            try
            {
                _listener.Start();
                _running = true;
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
            _running = false;
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
                requestQueue.Enqueue(context);

                Receive();
            }
        }
    }
}
