using System.Collections.Generic;

namespace MySharpChat.Client.Input
{
    public interface IUserInputTextHandler
    {
        /// <summary>
        /// Gets the number of elements contained in the <see cref="MySharpChat.Client.Input.IUserInputTextHandler"/>
        /// </summary>
        /// <returns></returns>
        int Length { get; }
        /// <summary>
        /// Adds an char to the end of the <see cref="MySharpChat.Client.Input.IUserInputTextHandler"/>
        /// </summary>
        /// <param name="c"></param>
        void Append(char c);
        /// <summary>
        /// Adds an list of cahrs to the end of the <see cref="MySharpChat.Client.Input.IUserInputTextHandler"/>
        /// </summary>
        /// <param name="c"></param>
        void Append(IEnumerable<char> chars);
        /// <summary>
        /// Inserts an element into the <see cref="MySharpChat.Client.Input.IUserInputTextHandler"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="c"></param>
        /// <exception cref="System.ArgumentOutOfRangeException">index is less than 0. -or- index is equal to or greater than <see cref="MySharpChat.Client.Input.IUserInputTextHandler"/> number of elements</exception>
        void InsertAt(int index, char c);
        /// <summary>
        /// Removes the element at the specified index of the <see cref="MySharpChat.Client.Input.IUserInputTextHandler"/>.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">index is less than 0. -or- index is equal to or greater than <see cref="MySharpChat.Client.Input.IUserInputTextHandler"/> number of elements</exception>
        void RemoveAt(int index);
        /// <summary>
        /// Removes a range of elements from the <see cref="MySharpChat.Client.Input.IUserInputTextHandler"/>
        /// </summary>
        /// <param name="index">The zero-based starting index of the range of elements to remove.</param>
        /// <param name="count">The number of elements to remove.</param>
        /// <exception cref="System.ArgumentOutOfRangeException">index is less than 0. -or- count is less than 0.</exception>
        /// <exception cref="System.ArgumentException">index and count do not denote a valid range of elements in the <see cref="MySharpChat.Client.Input.IUserInputTextHandler"/>.</exception>
        void RemoveRange(int index, int count);
        /// <summary>
        /// Removes all elements from the <see cref="MySharpChat.Client.Input.IUserInputTextHandler"/>
        /// </summary>
        void Clear();

        string ToString();
    }
}
