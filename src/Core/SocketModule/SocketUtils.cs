using MySharpChat.Core.Utils;
using MySharpChat.Core.Utils.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MySharpChat.Core.SocketModule
{
    public class SocketUtils
    {
        private static readonly Logger logger = Logger.Factory.GetLogger<SocketUtils>();

        private SocketUtils() { }

        public static Socket OpenListener(ConnexionInfos.Data data)
        {
            if (data.Ip == null)
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one 
                throw new ArgumentNullException(nameof(data.Ip));
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one 

            // Create a TCP/IP socket.  
            Socket listener = new Socket(data.Ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            return listener;
        }

        public static void ShutdownListener(Socket? socket)
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

        public static bool IsConnected(Socket? socket)
        {
            if (socket == null)
                throw new ArgumentNullException(nameof(socket));

            return socket.Connected;
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

                if(bytesRead > 0)
                {
                    // There  might be more data, so store the data received so far.
                    string dataStr = Encoding.ASCII.GetString(state.buffer, 0, bytesRead);
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

        public static bool Send(Socket? handler, string data, object? caller = null)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Encoding.ASCII.GetBytes(data);

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

        public static Tuple<IEnumerable<IPAddress>, IEnumerable<IPAddress>> GetAvailableIpAdresses(string? hostname)
        {
#if DEBUG
            string actualHostName = hostname ?? "localhost";
#else
            string actualHostName = hostname ?? Dns.GetHostName();
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
