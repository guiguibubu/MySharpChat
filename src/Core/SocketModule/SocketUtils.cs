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
        private static Logger logger = Logger.Factory.GetLogger<SocketUtils>();

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
            }
            catch (SocketException)
            {
                //If socket is not connected can't be shutdown
            }
            finally
            {
                socket.Disconnect(false);
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

        public static string Read(Socket? handler, object? caller = null)
        {
            return ReadAsync(handler, caller).GetAwaiter().GetResult();
        }

        public static Task<string> ReadAsync(Socket? handler, object? caller = null)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            string content = string.Empty;

            // Create the state object.  
            SocketContext state = new SocketContext();
            state.workSocket = handler;
            state.owner = caller;
            var tcs = new TaskCompletionSource<string>();
            Task<int> bytesRead = handler.ReceiveAsync(new ArraySegment<byte>(state.buffer, 0, SocketContext.BUFFER_SIZE), 0);
            bytesRead.GetAwaiter().OnCompleted(() =>
                {
                    try
                    {
                        ReadCallback(state, bytesRead.Result);

                        content = state.dataStringBuilder.ToString();

                        tcs.TrySetResult(content);
                    }
                    catch (OperationCanceledException)
                    {
                        tcs.TrySetCanceled();
                    }
                    catch (Exception e)
                    {
                        tcs.TrySetException(e);
                    }
                }
            );
            return tcs.Task;
        }

        private static void ReadCallback(SocketContext state, int bytesRead)
        {
            if (state.workSocket == null)
#pragma warning disable S3928 // Parameter names used into ArgumentException constructors should match an existing one 
                throw new ArgumentNullException(nameof(state.workSocket));
#pragma warning restore S3928 // Parameter names used into ArgumentException constructors should match an existing one 

            Socket handler = state.workSocket;

            if (handler.Connected
                && bytesRead > 0)
            {
                // There  might be more data, so store the data received so far.
                string dataStr = Encoding.ASCII.GetString(state.buffer, 0, bytesRead);
                state.dataStringBuilder.Append(dataStr);

                bool continueReceive = bytesRead == SocketContext.BUFFER_SIZE;
                if (continueReceive)
                {
                    // Not all data received. Get more.
                    Task<int> bytesReadAsyns = handler.ReceiveAsync(new ArraySegment<byte>(state.buffer, 0, SocketContext.BUFFER_SIZE), 0);
                    ReadCallback(state, bytesReadAsyns.GetAwaiter().GetResult());
                }
            }
        }

        public static bool Send(Socket? handler, string data, object? caller = null)
        {
            return SendAsync(handler, data, caller).GetAwaiter().GetResult();
        }

        public static Task<bool> SendAsync(Socket? handler, string data, object? caller = null)
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

            var tcs = new TaskCompletionSource<bool>();

            if (handler.Connected)
            {
                // Begin sending the data to the remote device.  
                Task<int> bytesSent = handler.SendAsync(new ArraySegment<byte>(byteData, 0, byteData.Length), 0);
                bytesSent.GetAwaiter().OnCompleted(() =>
                    {
                        try
                        {
                            tcs.TrySetResult(true);
                        }
                        catch (OperationCanceledException)
                        {
                            tcs.TrySetCanceled();
                        }
                        catch (Exception e)
                        {
                            tcs.TrySetException(e);
                        }
                        EndPoint remoteEP = handler.RemoteEndPoint!;
                        logger.LogDebug("Send {0} bytes to {1}. Data :{2}", bytesSent.Result, remoteEP, data);
                    }
                );

            }
            else
            {
                logger.LogError("Can not send data. Socket is disconnect.");
                tcs.TrySetResult(false);
            }

            return tcs.Task;
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
