namespace MySharpChat.Client.Input
{
    internal interface IUserInputCursorHandler
    {
        int Position { get; }

        void MovePositionPositive(int move, CursorUpdateModeEnum cursorUpdateMode = CursorUpdateModeEnum.Normal);

        void MovePositionNegative(int move, CursorUpdateModeEnum cursorUpdateMode = CursorUpdateModeEnum.Normal);

        void MovePositionToOrigin(CursorUpdateModeEnum cursorUpdateMode = CursorUpdateModeEnum.Normal) => MovePositionNegative(Position, cursorUpdateMode);

        void MovePositionToTail(int textLength, CursorUpdateModeEnum cursorUpdateMode = CursorUpdateModeEnum.Normal);
    }
}
