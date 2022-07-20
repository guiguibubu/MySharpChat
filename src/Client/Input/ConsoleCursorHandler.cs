namespace MySharpChat.Client.Input
{
    internal class ConsoleCursorHandler : IUserInputCursorHandler
    {
        private IUserInputCursorContext _context { get; set; }

        public ConsoleCursorHandler(IUserInputCursorContext context)
        {
            _context = context;
        }

        public int Position { get; private set; }

        public void MovePositionNegative(int move, CursorUpdateModeEnum cursorUpdateMode = CursorUpdateModeEnum.Normal)
        {
            for (int i = 0; i < move; i++)
            {
                MovePositionNegative(cursorUpdateMode);
            }
        }

        public void MovePositionPositive(int move, CursorUpdateModeEnum cursorUpdateMode = CursorUpdateModeEnum.Normal)
        {
            for (int i = 0; i < move; i++)
            {
                MovePositionPositive(cursorUpdateMode);
            }
        }

        private void MovePositionPositive(CursorUpdateModeEnum cursorUpdateMode = CursorUpdateModeEnum.Normal)
        {
            if (cursorUpdateMode != CursorUpdateModeEnum.NoGraphic)
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

            if(cursorUpdateMode != CursorUpdateModeEnum.GraphicalOnly)
                Position++;
        }

        private void MovePositionNegative(CursorUpdateModeEnum cursorUpdateMode = CursorUpdateModeEnum.Normal)
        {
            if (cursorUpdateMode != CursorUpdateModeEnum.NoGraphic)
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

            if (cursorUpdateMode != CursorUpdateModeEnum.GraphicalOnly)
                Position--;
        }

        public void MovePositionToTail(int textLength, CursorUpdateModeEnum cursorUpdateMode = CursorUpdateModeEnum.Normal)
        {
            MovePositionPositive(textLength - Position, cursorUpdateMode);
        }
    }
}
