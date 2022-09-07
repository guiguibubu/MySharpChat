using MySharpChat.Core.Utils;
using MySharpChat.Core.Utils.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net.Mime;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection.Metadata;
using MySharpChat.Core.Http;

namespace MySharpChat.Core.NetworkModule
{
    public class NetworkUtils
    {
        private static readonly Logger logger = Logger.Factory.GetLogger<NetworkUtils>();
        public static readonly Encoding Encoding = Encoding.ASCII;

        private NetworkUtils() { }

        [Obsolete("No more use of raw sockets and TCP connections")]
        public static Socket CreateSocket(ConnexionInfos.Data data)
        {
            if (data.Ip == null)
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one 
                throw new ArgumentNullException(nameof(data.Ip));
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one 

            // Create a TCP/IP socket.  
            Socket listener = new Socket(data.Ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            return listener;
        }

        [Obsolete("No more use of raw sockets and TCP connections")]
        public static void ShutdownSocket(Socket? socket)
        {
            if (socket == null)
                throw new ArgumentNullException(nameof(socket));

            try
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Disconnect(false);
            }
            catch (SocketException)
            {
                //If socket is not connected can't be shutdown
            }
            catch (ObjectDisposedException)
            {
                //If socket is already disposed
            }
            finally
            {
                socket.Close();
            }
        }

        [Obsolete("No more use of raw sockets and TCP connections")]
        //https://stackoverflow.com/questions/2661764/how-to-check-if-a-socket-is-connected-disconnected-in-c
        //https://github.com/jchristn/SuperSimpleTcp/blob/5c4bfbef56dd7a5a2e437f17ac62450f26feb3bf/src/SuperSimpleTcp/SimpleTcpClient.cs
        public static bool IsConnected(Socket? socket)
        {
            if (socket == null)
                throw new ArgumentNullException(nameof(socket));

            try
            {
                bool hasToReadOrDisconnected = socket.Poll(0, SelectMode.SelectRead);
                bool noDataToRead = socket.Available == 0;
                bool isDisconnected = hasToReadOrDisconnected && noDataToRead;

                if (hasToReadOrDisconnected)
                {
                    //Update socket connection status
                    byte[] buff = new byte[1];
                    socket.Receive(buff, SocketFlags.Peek);
                }

                return socket.Connected && !isDisconnected;
            }
            catch (ObjectDisposedException e)
            {
                logger.LogError(e, "Fail to detect connection status");
                return false;
            }
            catch (SocketException e)
            {
                logger.LogError(e, "Fail to detect connection status");
                return false;
            }
        }

        [Obsolete("No more use of raw sockets and TCP connections")]
        public static bool IsConnected(TcpClient? tcpClient)
        {
            if (tcpClient == null)
                throw new ArgumentNullException(nameof(tcpClient));

            return tcpClient.Connected && IsConnected(tcpClient.Client);
        }

        [Obsolete("No more use of raw sockets and TCP connections")]
        public static bool IsConnectionPending(Socket? socket)
        {
            if (socket == null)
                throw new ArgumentNullException(nameof(socket));

            try
            {
                return socket.Poll(100000, SelectMode.SelectRead);
            }
            catch (ObjectDisposedException)
            {
                return false;
            }
        }

        [Obsolete("No more use of raw sockets and TCP connections")]
        public static bool IsConnectionPending(TcpListener? tcpListener)
        {
            if (tcpListener == null)
                throw new ArgumentNullException(nameof(tcpListener));

            return tcpListener.Pending();
        }

        public static IPEndPoint CreateEndPoint(ConnexionInfos.Data data)
        {
            if (data.Ip == null)
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one 
                throw new ArgumentNullException(nameof(data.Ip));
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one 

            IPEndPoint endPoint = new IPEndPoint(data.Ip, data.Port);
            return endPoint;
        }

        [Obsolete("No more use of raw sockets and TCP connections")]
        public static string Read(Socket? handler, object? caller = null, CancellationToken cancelToken = default)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            // Create the state object.  
            SocketContext state = new SocketContext();
            state.workSocket = handler;
            state.owner = caller;

            while (handler.Available == 0)
            {
                Thread.SpinWait(100);
                cancelToken.ThrowIfCancellationRequested();
            }

            ReadImpl(state, cancelToken);

            string content = state.dataStringBuilder.ToString();
            return content;
        }

        [Obsolete("No more use of raw sockets and TCP connections")]
        public static string Read(TcpClient? tcpClient, CancellationToken cancelToken = default)
        {
            if (tcpClient == null)
                throw new ArgumentNullException(nameof(tcpClient));

            while (tcpClient.Available == 0)
            {
                Thread.SpinWait(100);
                cancelToken.ThrowIfCancellationRequested();
            }

            string content = string.Empty;
            try
            {
                NetworkStream stream = tcpClient.GetStream();
                int bytesRead;
                // Size of receive buffer.  
                const int BUFFER_SIZE = 256;
                // Receive buffer.  
                byte[] buffer = new byte[BUFFER_SIZE];
                // Received data string.  
                StringBuilder dataStringBuilder = new StringBuilder();

                while (stream.DataAvailable && (bytesRead = stream.Read(buffer, 0, buffer.Length)) != 0 && !cancelToken.IsCancellationRequested)
                {
                    // There  might be more data, so store the data received so far.
                    string dataStr = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    dataStringBuilder.Append(dataStr);
                }
                content = dataStringBuilder.ToString();
            }
            catch (OperationCanceledException) { }
            return content;
        }

        public static HttpResponseMessage Read(HttpClient httpCLient, HttpReadRequestContext context, CancellationToken cancelToken = default)
        {
            return httpCLient.GetAsync(context.Uri, cancelToken).Result;
        }

        [Obsolete("No more use of raw sockets and TCP connections")]
        public static Task<string> ReadAsync(Socket? handler, object? caller = null, CancellationToken cancelToken = default)
        {
            return Task.Factory.StartNew(
                () =>
                {
                    string content = string.Empty;
                    try { content = Read(handler, caller, cancelToken); }
                    catch (OperationCanceledException) { }
                    return content;
                }
            , cancelToken);
        }

        [Obsolete("No more use of raw sockets and TCP connections")]
        public static Task<string> ReadAsync(TcpClient? tcpClient, CancellationToken cancelToken = default)
        {
            return Task.Factory.StartNew(
                () =>
                {
                    string content = string.Empty;
                    try { content = Read(tcpClient, cancelToken); }
                    catch (OperationCanceledException) { }
                    return content;
                }
            , cancelToken);
        }

        public static Task<HttpResponseMessage?> ReadAsync(HttpClient httpCLient, HttpReadRequestContext context, CancellationToken cancelToken = default)
        {
            return Task.Factory.StartNew(
                () =>
                {
                    HttpResponseMessage? response = null;
                    try { response = Read(httpCLient, context, cancelToken); }
                    catch (OperationCanceledException) { }
                    return response;
                }
            , cancelToken);
        }

        [Obsolete("No more use of raw sockets and TCP connections")]
        private static void ReadImpl(SocketContext state, CancellationToken cancelToken = default)
        {
            if (state.workSocket == null)
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one 
                throw new ArgumentNullException(nameof(state.workSocket));
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one 

            Socket handler = state.workSocket!;

            if (handler.Connected)
            {

                int bytesRead = handler.Receive(new ArraySegment<byte>(state.buffer, 0, SocketContext.BUFFER_SIZE), 0);

                if (bytesRead > 0)
                {
                    // There  might be more data, so store the data received so far.
                    string dataStr = Encoding.GetString(state.buffer, 0, bytesRead);
                    state.dataStringBuilder.Append(dataStr);

                    bool continueReceive = bytesRead == SocketContext.BUFFER_SIZE;
                    if (continueReceive && !cancelToken.IsCancellationRequested)
                    {
                        // Not all data received. Get more.
                        ReadImpl(state, cancelToken);
                    }
                }
            }
        }

        [Obsolete("No more use of raw sockets and TCP connections")]
        public static bool Send(Socket? handler, string data, object? caller = null)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Encoding.GetBytes(data);

            // Create the state object.  
            SocketContext state = new SocketContext();
            state.workSocket = handler;
            state.owner = caller;
            state.dataStringBuilder.Clear();
            state.dataStringBuilder.Append(data);

            bool success;

            if (handler.Connected)
            {
                // Begin sending the data to the remote device.  
                int bytesSent = handler.Send(new ArraySegment<byte>(byteData, 0, byteData.Length), 0);
                EndPoint remoteEP = handler.RemoteEndPoint!;
                logger.LogDebug("Send {0} bytes to {1}. Data :{2}", bytesSent, remoteEP, data);
                success = true;
            }
            else
            {
                logger.LogError("Can not send data. Socket is disconnect.");
                success = false;
            }

            return success;
        }

        [Obsolete("No more use of raw sockets and TCP connections")]
        public static bool Send(TcpClient? tcpClient, string data)
        {
            if (tcpClient == null)
                throw new ArgumentNullException(nameof(tcpClient));

            bool success;

            if (IsConnected(tcpClient))
            {
                // Begin sending the data to the remote device.  
                NetworkStream stream = tcpClient.GetStream();
                // Convert the string data to byte data using ASCII encoding.  
                byte[] byteData = Encoding.GetBytes(data);
                stream.Write(byteData);

                EndPoint remoteEP = tcpClient.Client.RemoteEndPoint!;
                logger.LogDebug("Send {0} bytes to {1}. Data :{2}", byteData.Length, remoteEP, data);
                success = true;
            }
            else
            {
                logger.LogError("Can not send data. Socket is disconnect.");
                success = false;
            }

            return success;
        }

        public static HttpResponseMessage Send(HttpClient httpCLient, HttpSendRequestContext context, string data, CancellationToken cancelToken = default)
        {
            HttpContent httpRequestContent = new StringContent(data);
            httpRequestContent.Headers.ContentType = MediaTypeHeaderValue.Parse(MediaTypeNames.Application.Json);
            logger.LogInfo("Request send : {0}", data);
            if (context.HttpMethod == HttpMethod.Put)
                return httpCLient.PutAsync(context.Uri, httpRequestContent, cancelToken).Result;
            else if (context.HttpMethod == HttpMethod.Post)
                return httpCLient.PostAsync(context.Uri, httpRequestContent, cancelToken).Result;
            else if (context.HttpMethod == HttpMethod.Delete)
                return httpCLient.DeleteAsync(context.Uri, cancelToken).Result;
            else if (context.HttpMethod == HttpMethod.Patch)
                return httpCLient.PatchAsync(context.Uri, httpRequestContent, cancelToken).Result;
            else
                throw new InvalidOperationException(string.Format("{0} must be PUT, POST, DELETE or PATCH", nameof(context.HttpMethod)));
        }

        [Obsolete("No more use of raw sockets and TCP connections")]
        public static Task<bool> SendAsync(Socket? handler, string data, object? caller = null, CancellationToken cancelToken = default)
        {
            return Task.Factory.StartNew(
                () =>
                {
                    bool success = false;
                    try { success = Send(handler, data, caller); }
                    catch (OperationCanceledException) { }
                    return success;
                }
            , cancelToken);
        }

        [Obsolete("No more use of raw sockets and TCP connections")]
        public static Task<bool> SendAsync(TcpClient? tcpClient, string data, CancellationToken cancelToken = default)
        {
            return Task.Factory.StartNew(
                () =>
                {
                    bool success = false;
                    try { success = Send(tcpClient, data); }
                    catch (OperationCanceledException) { }
                    return success;
                }
            , cancelToken);
        }

        public static Task<HttpResponseMessage?> SendAsync(HttpClient httpCLient, HttpSendRequestContext context, string data, CancellationToken cancelToken = default)
        {
            return Task.Factory.StartNew(
                () =>
                {
                    HttpResponseMessage? response = null;
                    try { response = Send(httpCLient, context, data); }
                    catch (OperationCanceledException) { }
                    return response;
                }
            , cancelToken);
        }

        public static Tuple<IEnumerable<IPAddress>, IEnumerable<IPAddress>> GetAvailableIpAdresses(string? hostname)
        {
            string actualHostName =
                !string.IsNullOrEmpty(hostname) ?
                hostname
#if DEBUG
                : "localhost";
#else
                : Dns.GetHostName();
#endif

            IPHostEntry ipHostInfo = Dns.GetHostEntry(actualHostName);
            List<IPAddress> ipAddressesHost = ipHostInfo.AddressList.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork).ToList(); //Keep only IPv4

#if DEBUG
            List<IPAddress> ipAddressesNonVirtual = ipAddressesHost.ToList();
#else
            List<NetworkInterface> networkInterfaces = NetworkInterface.GetAllNetworkInterfaces()
                .Where(ni => ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet) //WiFI or Ethernet
                .Where(ni => ni.GetIPProperties().GatewayAddresses.FirstOrDefault() != null) //Virtual (like VirtualBox) network interfaces does not have Gateway address
                .ToList();

            List<IPAddress> ipAddressesNonVirtual = networkInterfaces!.Select(ni => ni.GetIPProperties()).SelectMany(ipprop => ipprop.UnicastAddresses).Select(uniAddr => uniAddr.Address).ToList();
#endif

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Available ip adresses");
            foreach (IPAddress ipAddress in ipAddressesHost)
            {
                sb.AppendLine(ipAddress.ToString());
            }
            sb.AppendLine("Available ip adresses non virtual");
            foreach (IPAddress ipAddress in ipAddressesNonVirtual)
            {
                sb.AppendLine(ipAddress.ToString());
            }
            logger.LogDebug(sb.ToString());

            return Tuple.Create((IEnumerable<IPAddress>)ipAddressesHost, (IEnumerable<IPAddress>)ipAddressesNonVirtual);
        }
    }
}
