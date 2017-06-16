using System;
using System.Collections.Generic;
using EW.Traits;
using EW.NetWork;
using EW.Primitives;
namespace EW
{
    public enum WorldT
    {
        Regular,
        Shellmap,
        Editor,
    }
    /// <summary>
    ///  ¿ΩÁ
    /// </summary>
    public sealed class World:IDisposable
    {
        public readonly Actor WorldActor;
        public readonly Map Map;
        public readonly WorldT Type;
        public readonly ActorMap ActorMap;
        public readonly ScreenMap ScreenMap;

        internal readonly TraitDictionary TraitDict = new TraitDictionary();
        internal readonly OrderManager OrderManager;


        readonly Queue<Action<World>> frameEndActions = new Queue<Action<World>>();

        readonly SortedDictionary<uint, Actor> actors = new SortedDictionary<uint, Actor>();
        uint nextAID = 0;

        public event Action<Actor> ActorAdded = _ => { };
        public event Action<Actor> ActorRemoved = _ => { };

        public Player[] Players = new Player[0];

        public bool ShouldTick { get { return Type != WorldT.Shellmap; } }
        internal World(Map map,OrderManager orderManager,WorldT type)
        {
            Type = type;
            OrderManager = orderManager;
            Map = map;

            var worldActorT = type == WorldT.Editor ? "EditorWorld" : "World";
            WorldActor = CreateActor(worldActorT, new TypeDictionary());
            ActorMap = WorldActor.Trait<ActorMap>();
            ScreenMap = WorldActor.Trait<ScreenMap>();

        }

        public void Tick()
        {
            while (frameEndActions.Count != 0)
                frameEndActions.Dequeue()(this);
        }

        public void AddFrameEndTask(Action<World> a)
        {
            frameEndActions.Enqueue(a);
        }

        public Actor CreateActor(string name,TypeDictionary initDict)
        {
            return CreateActor(true, name, initDict);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="addToWorld"></param>
        /// <param name="name"></param>
        /// <param name="initDict"></param>
        /// <returns></returns>
        public Actor CreateActor(bool addToWorld,string name,TypeDictionary initDict)
        {
            var a = new Actor(this, name, initDict);
            foreach(var t in a.TraitsImplementing<INotifyCreated>())
            {
                t.Created(a);
            }
            if (addToWorld)
            {
                Add(a);
            }
            return a;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        public void Add(Actor a)
        {
            a.IsInWorld = true;
            actors.Add(a.ActorID, a);
            ActorAdded(a);

            foreach(var t in a.TraitsImplementing<INotifyAddToWorld>())
            {
                t.AddedToWorld(a);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        public void Remove(Actor a)
        {
            a.IsInWorld = false;
            actors.Remove(a.ActorID);
            ActorRemoved(a);

            foreach (var t in a.TraitsImplementing<INotifyRemovedFromWorld>())
                t.RemovedFromWorld(a);
        }


        internal uint NextAID()
        {
            return nextAID++;
        }

        public void Dispose()
        {
        }
    }

    public struct TraitPair<T> : IEquatable<TraitPair<T>>
    {
        public readonly Actor Actor;

        public readonly T Trait;

        public TraitPair(Actor actor,T trait)
        {
            Actor = actor;
            Trait = trait;
        }

        public static bool operator ==(TraitPair<T> me,TraitPair<T> other) { return me.Actor == other.Actor && Equals(me.Trait, other.Trait); }

        public static bool operator !=(TraitPair<T> me,TraitPair<T> other) { return !(me == other); }

        public bool Equals(TraitPair<T> other) { return this == other; }

        public override bool Equals(object obj)
        {
            return obj is TraitPair<T> && Equals((TraitPair<T>)obj);
        }
        public override int GetHashCode()
        {
            return Actor.GetHashCode() ^ Trait.GetHashCode();
        }

        public override string ToString()
        {
            return "{0}->{1}".F(Actor.Info.Name, Trait.GetType().Name);
        }
    }
}