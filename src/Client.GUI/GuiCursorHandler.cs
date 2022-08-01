using MySharpChat.Core.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySharpChat.Client.GUI
{
    internal class GuiCursorHandler : IUserInputCursorHandler
    {
        public int Position => throw new NotImplementedException();

        public void MovePositionNegative(int move, CursorUpdateMode cursorUpdateMode = CursorUpdateMode.Normal)
        {
            throw new NotImplementedException();
        }

        public void MovePositionPositive(int move, CursorUpdateMode cursorUpdateMode = CursorUpdateMode.Normal)
        {
            throw new NotImplementedException();
        }

        public void MovePositionToTail(int textLength, CursorUpdateMode cursorUpdateMode = CursorUpdateMode.Normal)
        {
            throw new NotImplementedException();
        }
    }
}
