using MySharpChat.Core.Event;
using MySharpChat.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

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
