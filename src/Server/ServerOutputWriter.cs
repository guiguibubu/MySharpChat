﻿using MySharpChat.Core.Utils;
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
        public ServerOutputWriter(LockTextWriter output) : base(output)
        {
        }
    }
}
