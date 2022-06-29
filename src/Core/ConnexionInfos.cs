using System;
using System.Net;
using System.Text;

namespace MySharpChat
{
    public class ConnexionInfos
    {
        public const int DEFAULT_PORT = 11000;
        public string Hostname;
        public IPAddress Ip;
        public int Port = DEFAULT_PORT;
    }
}
