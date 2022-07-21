using System.Net;

namespace MySharpChat
{
    public class ConnexionInfos
    {
        public const int DEFAULT_PORT = 11000;

        public ConnexionInfos() : this(new Data(), new Data()) { }
        public ConnexionInfos(Data local, Data remote) {
            Local = local;
            Remote = remote;
        }

        public Data? Local = null;
        public Data? Remote = null;

        public class Data
        {
            public string? Hostname = null;
            public IPAddress? Ip = null;
            public int Port = DEFAULT_PORT;
        }
    }
}
