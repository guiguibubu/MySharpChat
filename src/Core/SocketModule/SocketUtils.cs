using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace MySharpChat.Core.SocketModule
{
    public static class SocketUtils
    {
        public static Socket OpenListener(ConnexionInfos.Data data)
        {
            if (data.Ip == null)
                throw new ArgumentNullException("data.Ip");

            // Create a TCP/IP socket.  
            Socket listener = new Socket(data.Ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            return listener;
        }

        public static void ShutdownListener(Socket socket)
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }

        public static IPEndPoint CreateEndPoint(ConnexionInfos.Data data)
        {
            if (data.Ip == null)
                throw new ArgumentNullException("data.Ip");

            IPEndPoint endPoint = new IPEndPoint(data.Ip, data.Port);
            return endPoint;
        }

        public static string Read(Socket handler, AsyncCallback callback, object? caller = null, ManualResetEvent? receiveDone = null)
        {
            string content = string.Empty;

            // Create the state object.  
            SocketContext state = new SocketContext();
            state.workSocket = handler;
            state.owner = caller;
            state.receiveDone = receiveDone;
            handler.BeginReceive(state.buffer, 0, SocketContext.BUFFER_SIZE, 0, callback, state);

            receiveDone?.WaitOne();
            // Set the event to nonsignaled state.  
            receiveDone?.Reset();
            content = state.dataStringBuilder.ToString();

            return content;
        }

        public static void ReadCallback(IAsyncResult ar)
        {
            // Retrieve the state object and the handler socket  
            // from the asynchronous state object.  
            if (ar.AsyncState is SocketContext state
                && state.workSocket != null)
            {
                Socket handler = state.workSocket;

                if (handler.Connected)
                {
                    // Read data from the client socket.
                    int bytesRead = handler.EndReceive(ar);

                    bool readFinished = false;
                    if (bytesRead > 0)
                    {
                        // There  might be more data, so store the data received so far.
                        string dataStr = Encoding.ASCII.GetString(state.buffer, 0, bytesRead);
                        state.dataStringBuilder.Append(dataStr);

                        bool continueReceive = bytesRead == SocketContext.BUFFER_SIZE;
                        if (continueReceive)
                        {
                            // Not all data received. Get more.  
                            handler.BeginReceive(state.buffer, 0, SocketContext.BUFFER_SIZE, SocketFlags.None, ReadCallback, state);
                        }
                        readFinished = !continueReceive;
                    }
                    else
                    {
                        readFinished = true;
                    }

                    if (readFinished)
                    {
                        state.receiveDone?.Set();
                    }
                }
            }
        }

        public static bool Send(Socket handler, string data, AsyncCallback callback, object? caller = null, ManualResetEvent? sendDone = null)
        {
            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Create the state object.  
            SocketContext state = new SocketContext();
            state.workSocket = handler;
            state.owner = caller;
            state.sendDone = sendDone;
            state.dataStringBuilder.Clear();
            state.dataStringBuilder.Append(data);

            if (handler != null && handler.Connected)
            {
                // Begin sending the data to the remote device.  
                handler.BeginSend(byteData, 0, byteData.Length, 0, callback, state);
                return true;
            }
            else
            {
                Console.WriteLine("Can not send data. Socket is disconnect.");
                return false;
            }
        }

        public static int SendCallback(IAsyncResult ar, out string text)
        {
            int bytesSent = 0;
            text = "";

            try
            {
                if (ar.AsyncState is SocketContext state
                    && state.workSocket != null)
                {
                    // Retrieve the socket from the state object.  
                    Socket handler = state.workSocket;

                    // Complete sending the data to the remote device.  
                    bytesSent = handler.EndSend(ar);

                    text = state.dataStringBuilder.ToString();

                    state.sendDone?.Set();
                }
            }
            catch (Exception e)
            {
                bytesSent = 0;
                text = "";
                throw new ApplicationException("Fail to send datas", e);
            }

            return bytesSent;
        }

        public static Tuple<IEnumerable<IPAddress>,IEnumerable<IPAddress>> GetAvailableIpAdresses(string? hostname)
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

#if DEBUG
            Console.WriteLine("Available ip adresses");
            foreach (IPAddress ipAddress in ipAddressesHost)
            {
                Console.WriteLine("{0}", ipAddress);
            }
            Console.WriteLine("Available ip adresses non virtual");
            foreach (IPAddress ipAddress in ipAddressesNonVirtual)
            {
                Console.WriteLine("{0}", ipAddress);
            }
#endif
            return Tuple.Create((IEnumerable<IPAddress>) ipAddressesHost, (IEnumerable<IPAddress>)ipAddressesNonVirtual);
        }
    }
}
