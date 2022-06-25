using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MySharpChat
{
    // State object for reading client data asynchronously  
    public class SocketContext
    {
        // Client  socket.  
        public Socket workSocket = null;
        // Size of receive buffer.  
        public const int BUFFER_SIZE = 256;
        // Receive buffer.  
        public byte[] buffer = new byte[BUFFER_SIZE];
        // Received data string.  
        public StringBuilder dataStringBuilder = new StringBuilder();
    }
}
