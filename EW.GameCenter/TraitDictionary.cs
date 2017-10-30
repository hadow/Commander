using System;
using System.Collections;
using System.Collections.Generic;
namespace EW
{

    public static class TypeExts
    {
        public static IEnumerable<Type> BaseTypes(this Type t)
        {
            while (t != null)
            {
                yield return t;
                t = t.BaseType;
            }
        }
    }
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
        /// <summary>
        /// Trait container interface.
        /// </summary>
        interface ITraitContainer
        {
            void Add(Actor actor, object trait);
            void RemoveActor(uint actor);

            int Queries { get; }
        }
        /// <summary>
        /// Trait 容器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        class TraitContainer<T> : ITraitContainer
        {
            readonly List<Actor> actors = new List<Actor>();
            readonly List<T> traits = new List<T>();

            public void Add(Actor actor,object trait)
            {
                var insertIndex = actors.BinarySearchMany(actor.ActorID + 1);
                actors.Insert(insertIndex, actor);
                traits.Insert(insertIndex, (T)trait);
            }

            public T Get(uint actor)
            {
                var result = GetOrDefault(actor);
                if (result == null)
                    throw new InvalidOperationException("Actor does not have trait of type '{0}'".F(typeof(T)));
                return result;
            }


            public T GetOrDefault(uint actor)
            {
                Queries++;

                var index = actors.BinarySearchMany(actor);//二分查找
                if (index >= actors.Count || actors[index].ActorID != actor)
                    return default(T);
                else if (index + 1 < actors.Count && actors[index + 1].ActorID == actor)
                    throw new InvalidOperationException("Actor {0} has multiple traits of type '{1}'".F(actors[index].Info.Name, typeof(T)));
                else
                    return traits[index];
            }
           

            /// <summary>
            /// 
            /// </summary>
            /// <param name="actor"></param>
            public void RemoveActor(uint actor)
            {
                var startIndex = actors.BinarySearchMany(actor);
                if(startIndex >= actors.Count || actors[startIndex].ActorID != actor)
                {
                    return;
                }
                var endIndex = startIndex + 1;
                while (endIndex < actors.Count && actors[endIndex].ActorID == actor)
                    endIndex++;
                var count = endIndex - startIndex;
                actors.RemoveRange(startIndex, count);
                traits.RemoveRange(startIndex, count);
            }

            public IEnumerable<T> GetMultiple(uint actor)
            {
                ++Queries;
                return new MultipleEnumerable(this, actor);
            }

            class MultipleEnumerable : IEnumerable<T>
            {
                readonly TraitContainer<T> container;
                readonly uint actor;
                public MultipleEnumerable(TraitContainer<T> container,uint actor)
                {
                    this.container = container;
                    this.actor = actor;
                }

                public IEnumerator<T> GetEnumerator()
                {
                    return new MultipleEnumerator(this.container, this.actor);
                }

                IEnumerator IEnumerable.GetEnumerator()
                {
                    return GetEnumerator();
                }
            }

            class MultipleEnumerator : IEnumerator<T>
            {
                readonly List<Actor> actors;
                readonly List<T> traits;
                readonly uint actor;

                int index;
                public MultipleEnumerator(TraitContainer<T> container,uint actor)
                {
                    actors = container.actors;
                    traits = container.traits;
                    this.actor = actor;
                    Reset();

                }

                public void Reset()
                {
                    index = actors.BinarySearchMany(actor) - 1;//Searches the entire sorted List<T> for an element using the default comparer and returns the zero-based index of the element.
                }

                public bool MoveNext()
                {
                    return ++index < actors.Count && actors[index].ActorID == actor;
                }

                public T Current
                {
                    get { return traits[index]; }
                }
                object IEnumerator.Current
                {
                    get { return Current; }
                }

                public void Dispose() { }


            }
            /// <summary>
            /// 
            /// </summary>
            class AllEnumerable : IEnumerable<TraitPair<T>>
            {
                readonly TraitContainer<T> container;

                public AllEnumerable(TraitContainer<T> container)
                {
                    this.container = container;
                }

                public IEnumerator<TraitPair<T>> GetEnumerator()
                {
                    return new AllEnumerator(container);
                }

                IEnumerator IEnumerable.GetEnumerator()
                {
                    return GetEnumerator();
                }
            }

            class AllEnumerator : IEnumerator<TraitPair<T>>
            {
                readonly List<Actor> actors;
                readonly List<T> traits;

                int index;
                public AllEnumerator(TraitContainer<T> container)
                {
                    actors = container.actors;
                    traits = container.traits;
                    Reset();
                }

                public bool MoveNext()
                {
                    return ++index < actors.Count;
                }

                public TraitPair<T> Current
                {
                    get
                    {
                        return new TraitPair<T>(actors[index], traits[index]);
                    }
                }

                object IEnumerator.Current { get { return Current; } }

                public void Reset() { index = -1; }

                public void Dispose() { }

            }

            
            public int Queries { get; private set; }

            public IEnumerable<TraitPair<T>> All()
            {
                ++Queries;
                return new AllEnumerable(this);
            }
        }

        /// <summary>
        /// 创建Trait 容器
        /// </summary>
        static readonly Func<Type, ITraitContainer> CreateTraitContainer = t =>
            (ITraitContainer)typeof(TraitContainer<>).MakeGenericType(t).GetConstructor(Type.EmptyTypes).Invoke(null);

        readonly Dictionary<Type, ITraitContainer> traits = new Dictionary<Type, ITraitContainer>();

        public T Get<T>(Actor actor)
        {
            CheckDestroyed(actor);
            return InnerGet<T>().Get(actor.ActorID);
        }

        public T GetOrDefault<T>(Actor actor)
        {
            CheckDestroyed(actor);
            return InnerGet<T>().GetOrDefault(actor.ActorID);
        }

        ITraitContainer InnerGet(Type t)
        {
            return traits.GetOrAdd(t, CreateTraitContainer);
        }

        TraitContainer<T> InnerGet<T>()
        {
            return (TraitContainer<T>)InnerGet(typeof(T));
        }

        public void AddTrait(Actor actor,object val)
        {
            var t = val.GetType();

            foreach (var i in t.GetInterfaces())
                InnerAdd(actor, i, val);
            foreach (var tt in t.BaseTypes())
                InnerAdd(actor, tt, val);

        }

        void InnerAdd(Actor actor,Type t,object val)
        {
            InnerGet(t).Add(actor, val);
        }

        public IEnumerable<T> WithInterface<T>(Actor actor)
        {
            CheckDestroyed(actor);
            return InnerGet<T>().GetMultiple(actor.ActorID);
        }

        public IEnumerable<TraitPair<T>> ActorsWithTrait<T>()
        {
            return InnerGet<T>().All();
        }

        public void RemoveActor(Actor a)
        {
            foreach (var t in traits)
                t.Value.RemoveActor(a.ActorID);
        }

        static void CheckDestroyed(Actor actor)
        {
            if (actor.Disposed)
                throw new InvalidOperationException("Attempt to get trait from destroyed object {0}".F(actor));
        }
    }
}