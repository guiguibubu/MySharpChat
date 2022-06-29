using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MySharpChat.Core.SocketModule
{
    // State object for reading client data asynchronously  
    public class SocketContext
    {
        public ManualResetEvent? connectDone = null;
        public ManualResetEvent? sendDone = null;
        public ManualResetEvent? receiveDone = null;
        // Object owner of the socket
        public object? owner = null;
        // Client  socket.  
        public Socket? workSocket = null;
        // Size of receive buffer.  
        public const int BUFFER_SIZE = 256;
        // Receive buffer.  
        public byte[] buffer = new byte[BUFFER_SIZE];
        // Received data string.  
        public readonly StringBuilder dataStringBuilder = new StringBuilder();
    }
}
