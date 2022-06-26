﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MySharpChat.SocketModule
{
    public static class SocketUtils
    {
        public static Socket OpenListener(ConnexionInfos connexionInfos)
        {
            // Create a TCP/IP socket.  
            Socket listener = new Socket(connexionInfos.Ip.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);
            return listener;
        }

        public static IPEndPoint CreateEndPoint(ConnexionInfos connexionInfos)
        {
            IPEndPoint endPoint = new IPEndPoint(connexionInfos.Ip, connexionInfos.Port);
            return endPoint;
        }

        public static string Read(Socket handler, AsyncCallback callback, object caller = null, ManualResetEvent receiveDone = null)
        {
            string content = string.Empty;

            // Create the state object.  
            SocketContext state = new SocketContext();
            state.workSocket = handler;
            state.owner = caller;
            state.receiveDone = receiveDone;
            handler.BeginReceive(state.buffer, 0, SocketContext.BUFFER_SIZE, 0, callback, state);
            
            receiveDone?.WaitOne();
            content = state.dataStringBuilder.ToString();

            return content;
        }

        public static void ReadCallback(IAsyncResult ar)
        {
            // Retrieve the state object and the handler socket  
            // from the asynchronous state object.  
            SocketContext state = (SocketContext)ar.AsyncState;
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

        public static void Send(Socket handler, string data, AsyncCallback callback, object caller = null, ManualResetEvent sendDone = null)
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

            // Begin sending the data to the remote device.  
            handler.BeginSend(byteData, 0, byteData.Length, 0, callback, state);
        }

        public static int SendCallback(IAsyncResult ar, out string text)
        {
            int bytesSent;

            try
            {
                // Retrieve the socket from the state object.  
                SocketContext state = (SocketContext)ar.AsyncState;
                Socket handler = state.workSocket;

                // Complete sending the data to the remote device.  
                bytesSent = handler.EndSend(ar);

                text = state.dataStringBuilder.ToString();

                state.sendDone?.Set();
            }
            catch (Exception e)
            {
                bytesSent = 0;
                text = "";
                throw new ApplicationException("Fail to send datas", e);
            }

            return bytesSent;
        }
    }
}
