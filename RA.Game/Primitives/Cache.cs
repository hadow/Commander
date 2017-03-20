using System;
using System.Collections;
using System.Collections.Generic;
namespace RA.Game.Primitives
{
    public class Cache<T,U>:IReadOnlyDictionary<T,U>
    {
        readonly Dictionary<T, U> cache;
        readonly Func<T, U> loader;

        public Cache(Func<T,U> loader,IEqualityComparer<T> c)
        {
            if (loader == null)
                throw new ArgumentNullException("loader");

            this.loader = loader;
            cache = new Dictionary<T, U>(c);
        }

        public Cache(Func<T,U> loader):this(loader,EqualityComparer<T>.Default)
        {

        }


        public U this[T key]
        {
            get { return cache.GetOrAdd(key, loader); }
        }

        public bool ContainsKey(T key) { return cache.ContainsKey(key); }

        public bool TryGetValue(T key,out U value) { return cache.TryGetValue(key, out value); }

        public int Count { get { return cache.Count; } }

        public ICollection<T> Keys { get { return cache.Keys; } }

        public ICollection<U> Values { get { return cache.Values; } }


        public IEnumerator<KeyValuePair<T,U>> GetEnumerator() { return cache.GetEnumerator(); }

        IEnumerator IEnumerable.GetEnumerator() { return cache.GetEnumerator(); }


    }
}