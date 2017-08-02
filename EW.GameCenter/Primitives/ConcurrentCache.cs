using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace EW.Primitives
{
    /// <summary>
    /// ²¢·¢»º´æ
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="U"></typeparam>
    public class ConcurrentCache<T,U>:IReadOnlyDictionary<T,U>
    {
        readonly ConcurrentDictionary<T, U> cache;
        readonly Func<T, U> loader;

        public ConcurrentCache(Func<T,U> loader,IEqualityComparer<T> c)
        {
            if (loader == null)
                throw new ArgumentNullException("loader");

            this.loader = loader;

            cache = new ConcurrentDictionary<T, U>(c);
        }

        public ConcurrentCache(Func<T,U> loader) : this(loader, EqualityComparer<T>.Default)
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

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

    }
}