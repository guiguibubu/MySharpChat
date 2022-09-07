using MySharpChat.Core.Utils.Logger;
using System;
using System.Collections.Concurrent;
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

        private readonly HttpListener _listener;
        public ReadOnlyCollection<string> Prefixes => _listener.Prefixes.ToList().AsReadOnly();
        private bool _running = false;
        public bool IsRunning => _running;

        public readonly ConcurrentQueue<HttpListenerContext> requestQueue = new();

        public HttpServer()
        {
            _listener = new HttpListener();
        }

        public void Start(IPEndPoint? endpoint)
        {
            if (endpoint == null)
                throw new ArgumentNullException(nameof(endpoint));

            string httpAdresse = string.Format("http://{0}:{1}/", endpoint.Address.ToString(), endpoint.Port);
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
