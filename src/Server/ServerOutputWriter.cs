using MySharpChat.Core.Utils;

namespace MySharpChat.Server
{
    internal class ServerOutputWriter : LockTextWriter
    {
        public ServerOutputWriter(LockTextWriter output) : base(output)
        {
        }
    }
}
