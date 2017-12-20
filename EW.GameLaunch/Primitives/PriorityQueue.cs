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


        /// <summary>
        /// Above the specified level and index.
        /// </summary>
        /// <returns>The above.</returns>
        /// <param name="level">Level.</param>
        /// <param name="index">Index.</param>

        T Above(int level,int index){
            return items[level - 1][index >> 1];
        }




        public void Add(T item)
        {
            var addLevel = level;
            var addIndex = index;

            while(addLevel>=1 && comparer.Compare(Above(addLevel,addIndex),item)>0)
            {
                items[addLevel][addIndex] = Above(addLevel, addIndex);
                --addLevel;
                addIndex >>= 1;

            }

            items[addLevel][addIndex] = item;

            if(++index>=(1<<level))
            {
                index = 0;

                if(items.Count <=++level)
                {

                    items.Add(new T[1<<level]);
                }
            }

        }

        public T Peek()
        {
            if (level <= 0 && index <= 0)
                throw new InvalidOperationException("PriorityQueue empty.");
            return At(0, 0);
        }

        public T Pop()
        {

            var ret = Peek();
            BubbleInto(0,0,Last());

            if (--index < 0)
                index = (1 << --index) - 1;
            
            return ret;
        }

        /// <summary>
        /// Last this instance.
        /// </summary>
        /// <returns>The last.</returns>
        T Last(){

            var lastLevel = level;
            var lastIndex = index;

            if (--lastIndex < 0)
                lastIndex = (1 << --lastIndex) - 1;

            return At(lastLevel, lastIndex);
        }



        void BubbleInto(int intoLevel,int intoIndex,T val){

            var downLevel = intoLevel + 1;
            var downIndex = intoIndex << 1;

            if (downLevel > level || (downLevel == level && downIndex >= index)){

                items[intoLevel][intoIndex] = val;
                return;
            }

            if(downLevel <= level && downIndex < index-1 && comparer.Compare(At(downLevel,downIndex),At(downLevel,downIndex+1))>=0)
            {
                downIndex++;
            }


            if(comparer.Compare(val,At(downLevel,downIndex))<=0)
            {
                items[intoLevel][intoIndex] = val;
                return;
            }

            items[intoLevel][intoIndex] = At(downLevel, downIndex);
            BubbleInto(downLevel,downIndex,val);
        }

        public bool Empty { get { return level == 0; } }

    }
}