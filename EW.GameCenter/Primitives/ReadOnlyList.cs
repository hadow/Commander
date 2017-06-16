using System;
using System.Collections;
using System.Collections.Generic;


namespace EW
{

    public interface IReadOnlyList<out T> : IEnumerable<T>
    {
        int Count { get; }
        T this[int index] { get; }
    }

    public static class ReadOnlyList
    {
        public static IReadOnlyList<T> AsReadOnly<T>(this IList<T> list)
        {
            return list as IReadOnlyList<T> ?? new ReadOnlyList<T>(list);
        }
    }

    public class ReadOnlyList<T> : IReadOnlyList<T>
    {
        private readonly IList<T> list;

        public ReadOnlyList():this(new List<T>())
        {

        }

        public ReadOnlyList(IList<T> list)
        {
            if (list == null)
                throw new ArgumentNullException("list");
            this.list = list;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        public int Count { get { return list.Count; } }

        public T this[int index] { get { return list[index]; } }
             
    }


}