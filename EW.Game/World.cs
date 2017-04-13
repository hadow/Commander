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

        internal readonly TraitDictionary TraitDict = new TraitDictionary();
        internal readonly OrderManager OrderManager;


        readonly Queue<Action<World>> frameEndActions = new Queue<Action<World>>();

        readonly SortedDictionary<uint, Actor> actors = new SortedDictionary<uint, Actor>();
        uint nextAID = 0;

        public event Action<Actor> ActorAdded = _ => { };
        public event Action<Actor> ActorRemoved = _ => { };

        public bool ShouldTick { get { return Type != WorldT.Shellmap; } }
        internal World(Map map,OrderManager orderManager,WorldT type)
        {

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


        internal uint NextAID()
        {
            return nextAID++;
        }

        public void Dispose()
        {
        }
    }
}