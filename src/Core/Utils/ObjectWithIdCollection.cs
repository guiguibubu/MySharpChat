﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MySharpChat.Core.Utils
{
    [Serializable]
    public class ObjectWithIdCollection<T> : HashSet<T> where T : class, IObjectWithId
    {
        public ObjectWithIdCollection()
            : base()
        { }
        public ObjectWithIdCollection(IEnumerable<T> collection) 
            : base(collection)
        { }
        public ObjectWithIdCollection(IEqualityComparer<T>? comparer)
            : base(comparer)
        { }
        public ObjectWithIdCollection(int capacity)
            : base(capacity)
        { }
        public ObjectWithIdCollection(IEnumerable<T> collection, IEqualityComparer<T>? comparer)
            : base(collection, comparer)
        { }
        public ObjectWithIdCollection(int capacity, IEqualityComparer<T>? comparer)
            : base(capacity, comparer)
        { }


        protected ObjectWithIdCollection(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }

        public bool Contains(Guid id)
        {
            return this.Any((o) => o.Id == id);
        }

        public T Get(Guid id)
        {
            return this.First((o) => o.Id == id);
        }

        public bool TryGet(Guid id, out T? result)
        {
            result = this.FirstOrDefault((o) => o?.Id == id, null);
            return result != null;
        }

        public T this[Guid id]
        {
            get
            {
                return Get(id);
            }
        }

        public void Remove(Guid id)
        {
            RemoveWhere((o) => o?.Id == id);
        }
    }
}