using System;
using System.Collections.Generic;

namespace EW.Primitives
{
    public interface IPriorityQueue<T>
    {
        void Add(T item);

        bool Empty { get; }

        T Peek();

        T Pop();
    }
    public class PriorityQueue<T>:IPriorityQueue<T>
    {

        readonly List<T[]> items;

        readonly IComparer<T> comparer;

        int level, index;

        public PriorityQueue(IComparer<T> comparer)
        {
            items = new List<T[]> { new T[1] };
            this.comparer = comparer;
        }


        T At(int level,int index)
        {
            return items[level][index];
        }
        public void Add(T item)
        {

        }

        public T Peek()
        {
            if (level <= 0 && index <= 0)
                throw new InvalidOperationException("PriorityQueue empty.");
            return At(0, 0);
        }

        public T Pop()
        {
            throw new NotImplementedException();
        }

        public bool Empty { get { return level == 0; } }

    }
}