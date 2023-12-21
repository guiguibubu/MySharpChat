using System;

namespace MySharpChat.Core.Utils
{
    public interface IObjectWithId
    {
        Guid Id { get; }
    }
}
