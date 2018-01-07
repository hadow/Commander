using System;
using System.Linq;
using System.Collections.Generic;
using EW.Traits;
using EW.NetWork;
using EW.Primitives;
using EW.Support;
using EW.Graphics;
using EW.Effects;
namespace EW
{
    public enum WorldT
    {
        Regular,
        Shellmap,
        Editor,
    }
    /// <summary>
    /// 虚拟世界
    /// </summary>
    public sealed class World:IDisposable
    {
        public readonly Actor WorldActor;
        public readonly Map Map;
        public readonly WorldT Type;
        public readonly IActorMap ActorMap;
        public readonly ScreenMap ScreenMap;

        public readonly Selection Selection = new Selection();


        internal readonly TraitDictionary TraitDict = new TraitDictionary();
        internal readonly OrderManager OrderManager;

        readonly List<IEffect> effects = new List<IEffect>();
        readonly List<ISync> syncedEffects = new List<ISync>();

        //未分隔的特效
        readonly List<IEffect> unpartitionedEffects = new List<IEffect>();

        public readonly MersenneTwister SharedRandom;

        public readonly IModelCache ModelCache;
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

        IOrderGenerator orderGenerator;

        public IOrderGenerator OrderGenerator
        {
            get { return orderGenerator; }
            set
            {
                orderGenerator = value;
            }
        }

        public IEnumerable<Actor> Actors { get { return actors.Values; } }

        public IEnumerable<IEffect> Effects { get { return effects; } }


        public IEnumerable<IEffect> UnpartitionedEffects{ get { return unpartitionedEffects; }}

        public IEnumerable<ISync> SyncedEffects { get { return syncedEffects; }}
        public bool FogObscures(CPos p) { return RenderPlayer != null && !RenderPlayer.Shroud.IsVisible(p); }

        public bool FogObscures(WPos pos) { return RenderPlayer != null && !RenderPlayer.Shroud.IsVisible(pos); }

        /// <summary>
        /// Fogs the obscures.
        /// 雾效模糊
        /// </summary>
        /// <returns><c>true</c>, if obscures was foged, <c>false</c> otherwise.</returns>
        /// <param name="a">The alpha component.</param>
        public bool FogObscures(Actor a) { return RenderPlayer != null && !RenderPlayer.CanViewActor(a); }

        public Player LocalPlayer { get; private set; }

        public int WorldTick { get; private set; }

        /// <summary>
        /// 标识是否Tick
        /// </summary>
        public bool ShouldTick { get { return Type != WorldT.Shellmap || WarGame.Settings.Game.ShowShellmap; } }

        public bool Paused { get; internal set; }

        public bool IsReplay
        {
            get
            {
                return OrderManager.Connection is ReplayConnection;
            }
        }
        internal World(ModData modData,Map map,OrderManager orderManager,WorldT type)
        {
            Type = type;
            OrderManager = orderManager;
            Map = map;
            Timestep = orderManager.LobbyInfo.GlobalSettings.Timestep;
            SharedRandom = new MersenneTwister(orderManager.LobbyInfo.GlobalSettings.RandomSeed);

            ModelCache = modData.ModelSequenceLoader.CachModels(map, modData, map.Rules.ModelSequences);

            var worldActorT = type == WorldT.Editor ? "EditorWorld" : "World";
            WorldActor = CreateActor(worldActorT, new TypeDictionary());
            ActorMap = WorldActor.Trait<IActorMap>();
            ScreenMap = WorldActor.Trait<ScreenMap>();
            
            //Add players
            foreach (var cmp in WorldActor.TraitsImplementing<ICreatePlayers>())
                cmp.CreatePlayers(this);

            //Set defaults for any unset stances
            foreach (var p in Players)
                foreach (var q in Players)
                    if (!p.Stances.ContainsKey(q))
                        p.Stances[q] = Stance.Neutral;

        }

        public void SetPlayers(IEnumerable<Player> players,Player localPlayer)
        {
            if (Players.Length > 0)
                throw new InvalidOperationException("Players are fixed once they have been set.");
            Players = players.ToArray();
            SetLocalPlayer(localPlayer);
        }

        public void SetWorldOwner(Player p)
        {
            WorldActor.Owner = p;
        }

        void SetLocalPlayer(Player localPlayer)
        {
            if (localPlayer == null)
                return;

            if (!Players.Contains(localPlayer))
                throw new ArgumentException("The local player must be one of the players in the world.", "localPlayer");

            if (IsReplay)
                return;
            LocalPlayer = localPlayer;
            RenderPlayer = LocalPlayer;
        }

        /// <summary>
        /// 世界场景加载完成
        /// </summary>
        /// <param name="wr"></param>
        public void LoadComplete(WorldRenderer wr)
        {
            //ScreenMap must be initialized before anything else
            using (new PerfTimer("ScreenMap.WorldLoaded"))
                ScreenMap.WorldLoaded(this, wr);
            foreach(var wlh in WorldActor.TraitsImplementing<IWorldLoaded>())
            {
                if (wlh == ScreenMap)
                    continue;
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

            //帧结束回调
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

        public IEnumerable<Actor> ActorsHavingTrait<T>()
        {
            return TraitDict.ActorsHavingTrait<T>();
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
            ScreenMap.AddOrUpdate(self);
            //if(!self.Bounds.Size.IsEmpty)
                //ScreenMap.Add(self);
        }

        public void RemoveFromMaps(Actor self,IOccupySpace ios)
        {
            ActorMap.RemoveInfluence(self, ios);
            ActorMap.RemovePosition(self, ios);
            ScreenMap.Remove(self);
            //if(!self.Bounds.Size.IsEmpty)
                //ScreenMap.Remove(self);
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

            foreach(var t in a.TraitsImplementing<INotifyAddedToWorld>())
            {
                t.AddedToWorld(a);
            }
        }

        public void Add(IEffect e)
        {
            effects.Add(e);

            var se = e as ISync;
            if (se != null)
                syncedEffects.Add(se);
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


        public void Remove(IEffect e)
        {
            effects.Remove(e);
            var se = e as ISync;
            if (se != null)
            {
                syncedEffects.Remove(se);
            }
        }


        internal uint NextAID()
        {
            return nextAID++;
        }

        /// <summary>
        /// 更新地图
        /// </summary>
        /// <param name="self"></param>
        /// <param name="ios"></param>
        public void UpdateMaps(Actor self,IOccupySpace ios)
        {
            if (!self.IsInWorld)
                return;
            //if (!self.Bounds.Size.IsEmpty)
                //ScreenMap.Update(self);

            ScreenMap.AddOrUpdate(self);
            ActorMap.UpdatePosition(self, ios);

        }


        public int SyncHash()
        {
            var n = 0;
            var ret = 0;

            //Hash all the actors.
            foreach (var a in Actors)
                ret += n++ * (int)(1 + a.ActorID) * Sync.HashActor(a);

            //Hash fields marked with the ISync interface.
            foreach (var actor in ActorsHavingTrait<ISync>())
                foreach (var syncHash in actor.SyncHashes)
                    ret += n++ * (int)(1 + actor.ActorID) * syncHash.Hash;


            //Hash game state relevant effects such as projectiles.
            foreach (var sync in SyncedEffects)
                ret += n++ * Sync.Hash(sync);

            //Hash the shared random number generator.
            ret += SharedRandom.Last;
            return ret;
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