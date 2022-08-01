using MySharpChat.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySharpChat.Client.GUI
{
    internal class GuiOutputWriter : LockTextWriter
    {
        public GuiOutputWriter(TextWriter output) : base(output)
        {
        }
    }
}
