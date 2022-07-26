namespace MySharpChat.Client.Input
{
    internal interface IUserInputCursorHandler
    {
        int Position { get; }

        void MovePositionPositive(int move, CursorUpdateMode cursorUpdateMode = CursorUpdateMode.Normal);

        void MovePositionNegative(int move, CursorUpdateMode cursorUpdateMode = CursorUpdateMode.Normal);

        public virtual void MovePositionToOrigin(CursorUpdateMode cursorUpdateMode = CursorUpdateMode.Normal) => MovePositionNegative(Position, cursorUpdateMode);

        void MovePositionToTail(int textLength, CursorUpdateMode cursorUpdateMode = CursorUpdateMode.Normal);
    }
}
