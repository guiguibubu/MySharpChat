using System;
using System.Collections.Generic;
using MySharpChat.Core.Model;

namespace MySharpChat.Core.Utils.Collection
{
    [Serializable]
    public sealed class UserStateCollection : ObjectWithIdCollection<UserState>
    {
        public UserStateCollection()
            : base(UserState.Comparer)
        { }

        public UserStateCollection(IEnumerable<UserState> collection)
            : base(collection, UserState.Comparer)
        { }

        public UserStateCollection(int capacity)
            : base(capacity, UserState.Comparer)
        { }
    }
}
