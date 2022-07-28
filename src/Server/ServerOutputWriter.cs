using MySharpChat.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySharpChat.Server
{
    internal class ServerOutputWriter : LockTextWriter
    {
        private readonly LockTextWriter _output;

        public ServerOutputWriter(LockTextWriter output)
        {
            _output = output;
        }

        public override Encoding Encoding => _output.Encoding;

        public override void Write(char value)
        {
            _output.Write(value);
        }

        public override bool IsLocked => _output.IsLocked;

        public override IDisposable Lock()
        {
            return _output.Lock();
        }
    }
}
