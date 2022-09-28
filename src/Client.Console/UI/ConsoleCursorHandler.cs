namespace MySharpChat.Client.Console.UI
{
    public class ConsoleCursorHandler
    {
        public ConsoleCursorHandler()
        {
            Position = 0;
        }

        public int Position { get; private set; }

        public void MovePositionNegative(int move)
        {
            for (int i = 0; i < move; i++)
            {
                MovePositionNegative();
            }
        }

        public void MovePositionPositive(int move)
        {
            for (int i = 0; i < move; i++)
            {
                MovePositionPositive();
            }
        }

        private void MovePositionPositive()
        {
            Position++;
        }

        private void MovePositionNegative()
        {
            if (Position > 0)
                Position--;
        }

        public void MovePositionToOrigin()
        {
            MovePositionNegative(Position);
        }

        public void MovePositionToTail(int textLength)
        {
            MovePositionPositive(textLength - Position);
        }
    }
}
