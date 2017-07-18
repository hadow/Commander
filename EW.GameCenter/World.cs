using System;
using System.Linq;
using System.Collections.Generic;
using EW.Traits;
using EW.NetWork;
using EW.Primitives;
using EW.Support;
using EW.Graphics;
namespace EW
{
    public enum WorldT
    {
        Regular,
        Shellmap,
        Editor,
    }
    /// <summary>
    /// 世界
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

        public int Timestep;

        readonly Queue<Action<World>> frameEndActions = new Queue<Action<World>>();

        readonly SortedDictionary<uint, Actor> actors = new SortedDictionary<uint, Actor>();
        uint nextAID = 0;

        public event Action<Actor> ActorAdded = _ => { };
        public event Action<Actor> ActorRemoved = _ => { };

        public Player[] Players = new Player[0];

        Player renderPlayer;

        public Player RenderPlayer
        {
            get
            {
                return (renderPlayer == null || (renderPlayer.WinState != WinState.Undefined)) ? null : renderPlayer;
            }
            set
            {
                renderPlayer = value;
            }
        }

        public int WorldTick { get; private set; }

        /// <summary>
        /// 标识是否Tick
        /// </summary>
        public bool ShouldTick { get { return Type != WorldT.Shellmap || WarGame.Settings.Game.ShowShellmap; } }

        public bool Paused { get; internal set; }
        internal World(Map map,OrderManager orderManager,WorldT type)
        {
            Type = type;
            OrderManager = orderManager;
            Map = map;
            Timestep = orderManager.LobbyInfo.GlobalSettings.Timestep;
            var worldActorT = type == WorldT.Editor ? "EditorWorld" : "World";
            WorldActor = CreateActor(worldActorT, new TypeDictionary());
            ActorMap = WorldActor.Trait<ActorMap>();
            ScreenMap = WorldActor.Trait<ScreenMap>();

        }

        /// <summary>
        /// 加载完成
        /// </summary>
        /// <param name="wr"></param>
        public void LoadComplete(WorldRenderer wr)
        {
            using (new PerfTimer("ScreenMap.WorldLoaded"))
                ScreenMap.WorldLoaded(this, wr);
            foreach(var wlh in WorldActor.TraitsImplementing<IWorldLoaded>())
            {
                if (wlh == ScreenMap)
                    continue;
                Console.WriteLine("Name:" + wlh.GetType().Name);

                using (new PerfTimer(wlh.GetType().Name + ".WorldLoaded"))
                    wlh.WorldLoaded(this, wr);

            }
        }


        public void Tick()
        {
            if (!Paused)
            {
                WorldTick++;
                
                using(new PerfSample("tick_idle"))
                {
                    foreach(var ni in ActorsWithTrait<INotifyIdle>())
                    {
                        if (ni.Actor.IsIdle)
                        {
                            ni.Trait.TickIdle(ni.Actor);
                        }
                    }
                }

                using(new PerfSample("tick_activities"))
                {
                    foreach (var a in actors.Values)
                        a.Tick();
                }

                ActorsWithTrait<ITick>().DoTimed(x => x.Trait.Tick(x.Actor), "Trait");

            }

            //
            while (frameEndActions.Count != 0)
                frameEndActions.Dequeue()(this);
        }

        /// <summary>
        /// For things that want to update their render state once per tick.ignoring pause state
        /// </summary>
        /// <param name="wr"></param>
        public void TickRender(WorldRenderer wr)
        {
            ActorsWithTrait<ITickRender>().DoTimed(x => x.Trait.TickRender(wr, x.Actor), "Render");
        }

        public event Action GameOver = () => { };
        public bool IsGameOver { get; private set; }
        /// <summary>
        /// 结束游戏
        /// </summary>
        public void EndGame()
        {
            if (!IsGameOver)
            {
                IsGameOver = true;

                foreach(var t in WorldActor.TraitsImplementing<IGameOver>())
                {
                    t.GameOver(this);
                }

                GameOver();
            }
        }


        public bool PauseStateLocked { get; set; }

        public bool PredictedPaused { get; internal set; }
        /// <summary>
        /// 设置暂停
        /// </summary>
        /// <param name="paused"></param>
        public void SetPauseState(bool paused)
        {
            if (PauseStateLocked)
                return;
            IssueOrder(Order.PauseGame(paused));
            PredictedPaused = paused;
        }

        public void IssueOrder(Order order)
        {
            //Avoid exposing the OM to mod code;
            OrderManager.IssueOrder(order);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IEnumerable<TraitPair<T>> ActorsWithTrait<T>()
        {
            return TraitDict.ActorsWithTrait<T>();
        }

        public void AddFrameEndTask(Action<World> a)
        {
            frameEndActions.Enqueue(a);//将对象添加到Queue结尾处
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


        public void AddToMaps(Actor self,IOccupySpace ios)
        {
            ActorMap.AddInfluence(self, ios);
            ActorMap.AddPosition(self, ios);

            //if(!self.Bounds.Size.ise)
            ScreenMap.Add(self);
        }

        public void RemoveFromMaps(Actor self,IOccupySpace ios)
        {
            ActorMap.RemoveInfluence(self, ios);
            ActorMap.RemovePosition(self, ios);

            //if(!self.Bounds.Size.is)
            ScreenMap.Remove(self);
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

        public void UpdateMaps(Actor self,IOccupySpace ios)
        {
            if (!self.IsInWorld)
                return;
            if (!self.Bounds.Size.IsEmpty)
                ScreenMap.Update(self);

            ActorMap.UpdatePosition(self, ios);

        }

        public bool Disposing;

        public void Dispose()
        {
            Disposing = true;
            frameEndActions.Clear();

            //Dispose newer actors first,and the world actor last
            foreach (var a in actors.Values.Reverse())
                a.Dispose();

            while (frameEndActions.Count != 0)
                frameEndActions.Dequeue()(this);    //移除并返回位于Queue开始处的对象
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