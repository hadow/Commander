using System;
using System.Collections.Generic;
namespace EW
{
    static class ListExts
    {
        public static int BinarySearchMany(this List<Actor> list,uint searchFor)
        {
            var start = 0;
            var end = list.Count;
            while (start != end)
            {
                var mid = (start + end) / 2;
                if (list[mid].ActorID < searchFor)
                    start = mid + 1;
                else
                    end = mid;
            }
                
            return start;
        }
    }

    /// <summary>
    /// 提供一种高效的方法来查询Actors 的特性
    /// </summary>
    class TraitDictionary
    {

        interface ITraitContainer
        {
            void Add(Actor actor, object trait);
            void RemoveActor(uint actor);

            int Queries { get; }
        }

        class TraitContainer<T> : ITraitContainer
        {
            readonly List<Actor> actors = new List<Actor>();
            readonly List<T> traits = new List<T>();

            public void Add(Actor actor,object trait)
            {

            }

            public void RemoveActor(uint actor)
            {

            }
            public int Queries { get; private set; }
        }
    }
}