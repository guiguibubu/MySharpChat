using System.Net;

namespace MySharpChat
{
    public class ConnexionInfos
    {

#if DEBUG
        public const int HTTP_PORT = 8080;
#else
        public const int HTTP_PORT = 80;
#endif

        public const int DEFAULT_PORT = HTTP_PORT;

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
