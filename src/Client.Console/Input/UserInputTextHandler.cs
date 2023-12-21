using System.Collections.Generic;

namespace MySharpChat.Client.Console.Input
{
    public class UserInputTextHandler : IUserInputTextHandler
    {
        private readonly List<char> _chars = new List<char>();

        public void Append(char c)
        {
            _chars.Add(c);
        }

        public void Append(IEnumerable<char> chars)
        {
            foreach (char c in chars)
                _chars.Add(c);
        }

        public void Clear()
        {
            _chars.Clear();
        }

        public int Length => _chars.Count;

        public void InsertAt(int index, char c)
        {
            _chars.Insert(index, c);
        }

        public void RemoveAt(int index)
        {
            _chars.RemoveAt(index);
        }

        public void RemoveRange(int index, int count)
        {
            _chars.RemoveRange(index, count);
        }

        public override string ToString()
        {
            return new string(_chars.ToArray());
        }
    }
}
