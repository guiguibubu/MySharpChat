using MySharpChat.Core.UI;

namespace MySharpChat.Client.Console
{
    public class ConsoleCursorHandler : IUserInputCursorHandler
    {
        private IUserInputCursorContext _context { get; set; }

        public ConsoleCursorHandler(IUserInputCursorContext context)
        {
            _context = context;
            Position = 0;
        }

        public int Position { get; private set; }

        public void MovePositionNegative(int move, CursorUpdateMode cursorUpdateMode = CursorUpdateMode.Normal)
        {
            for (int i = 0; i < move; i++)
            {
                MovePositionNegative(cursorUpdateMode);
            }
        }

        public void MovePositionPositive(int move, CursorUpdateMode cursorUpdateMode = CursorUpdateMode.Normal)
        {
            for (int i = 0; i < move; i++)
            {
                MovePositionPositive(cursorUpdateMode);
            }
        }

        private void MovePositionPositive(CursorUpdateMode cursorUpdateMode = CursorUpdateMode.Normal)
        {
            if (cursorUpdateMode != CursorUpdateMode.NoGraphic)
            {
                int bufferWidth = _context.Width;
                int left = _context.X;
                int top = _context.Y;
                if (left == bufferWidth - 1)
                {
                    top++;
                    left = 0;
                }
                else
                {
                    left++;
                }
                _context.X = left;
                _context.Y = top;
            }

            if(cursorUpdateMode != CursorUpdateMode.GraphicalOnly)
                Position++;
        }

        private void MovePositionNegative(CursorUpdateMode cursorUpdateMode = CursorUpdateMode.Normal)
        {
            if (cursorUpdateMode != CursorUpdateMode.NoGraphic)
            {
                int bufferWidth = _context.Width;
                int left = _context.X;
                int top = _context.Y;
                if (left > 0)
                {
                    left--;
                }
                else
                {
                    top--;
                    left = bufferWidth - 1;
                }
                _context.X = left;
                _context.Y = top;
            }

            if (cursorUpdateMode != CursorUpdateMode.GraphicalOnly)
                Position--;
        }

        public void MovePositionToTail(int textLength, CursorUpdateMode cursorUpdateMode = CursorUpdateMode.Normal)
        {
            MovePositionPositive(textLength - Position, cursorUpdateMode);
        }
    }
}
