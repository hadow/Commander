using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace EW.Traits
{
    public enum SubCell
    {
        Invalid = int.MinValue,
        Any = int.MinValue/2,
        FullCell = 0,
        First = 1,
    }

    public class ActorMapInfo:ITraitInfo
    {
        public object Create(ActorInitializer init)
        {
            return new ActorMap(init.World, this);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class ActorMap:ITick
    {

        class InfluenceNode
        {
            public InfluenceNode Next;
            public SubCell SubCell;
            public Actor Actor;
        }

        /// <summary>
        /// 
        /// </summary>
        class CellTrigger
        {
            public readonly CellPos[] Footprint;
            public bool Dirty;

            readonly Action<Actor> onActorEntered;
            readonly Action<Actor> onActorExited;

            IEnumerable<Actor> currentActors = Enumerable.Empty<Actor>();
            
            public CellTrigger(CellPos[] footprint,Action<Actor> onActorEntered,Action<Actor> onActorExited)
            {
                Footprint = footprint;

                this.onActorEntered = onActorEntered;
                this.onActorExited = onActorExited;
                
                //通知所有最初在触发区内的Actor
                Dirty = true;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="actorMap"></param>
            public void Tick(ActorMap actorMap)
            {
                if (!Dirty)
                    return;

                var oldActors = currentActors;
                currentActors = Footprint.SelectMany(actorMap.GetActorsAt).ToList();

                var entered = currentActors.Except(oldActors);
                var exited = oldActors.Except(currentActors);

                if (onActorEntered != null)
                {
                    foreach (var a in entered)
                        onActorEntered(a);
                }

                if (onActorExited != null)
                {
                    foreach (var a in exited)
                        onActorExited(a);
                }
                Dirty = false;
            }

        }
        
        /// <summary>
        /// 
        /// </summary>
        class ProximityTrigger:IDisposable
        {
            public WorldPos TopLeft { get; private set; }

            public WorldPos BottomRight { get; private set; }

            public bool Dirty;

            readonly Action<Actor> onActorEntered;
            readonly Action<Actor> onActorExited;

            WorldPos position;
            WorldDist rang;
            WorldDist vRange;

            IEnumerable<Actor> currentActors = Enumerable.Empty<Actor>();


            public ProximityTrigger(WorldPos pos,WorldDist range,WorldDist vRange,Action<Actor> onActorEntered,Action<Actor> onActorExited)
            {
                this.onActorEntered = onActorEntered;
                this.onActorExited = onActorExited;
            }

            public void Tick(ActorMap am)
            {
                if (!Dirty)
                    return;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="newPos"></param>
            /// <param name="newRange"></param>
            /// <param name="newVRange"></param>
            public void Update(WorldPos newPos,WorldDist newRange,WorldDist newVRange)
            {
                position = newPos;
                rang = newRange;
                vRange = newVRange;

                var offset = new WorldVector(newRange, newRange, newVRange);

                TopLeft = newPos - offset;
                BottomRight = newPos + offset;

                Dirty = true;
            }

            public void Dispose()
            {
                if (onActorExited != null)
                {
                    foreach (var a in currentActors)
                        onActorExited(a);
                }
            }

        }

        class Bin
        {
            public readonly List<Actor> Actors = new List<Actor>();
            public readonly List<ProximityTrigger> ProximityTriggers = new List<ProximityTrigger>();
        }

        /// <summary>
        /// 
        /// </summary>
        sealed class ActorsAtEnumerator : IEnumerator<Actor>
        {
            InfluenceNode node;
            public Actor Current { get; private set; }
            object IEnumerator.Current { get { return Current; } }

            public ActorsAtEnumerator(InfluenceNode node)
            {
                this.node = node;
            }


            public void Reset() { throw new NotSupportedException(); }

            public bool MoveNext()
            {
                while(node != null)
                {
                    Current = node.Actor;
                    node = node.Next;
                    if (!Current.Disposed)
                        return true;
                }
                return false;
            }

            public void Dispose() { }
        }

        sealed class ActorsAtEnumerable : IEnumerable<Actor>
        {
            readonly InfluenceNode node;

            public ActorsAtEnumerable(InfluenceNode node) { this.node = node; }

            public IEnumerator<Actor> GetEnumerator()
            {
                return new ActorsAtEnumerator(node);
            }


            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }


        readonly ActorMapInfo info;
        readonly Map map;
        readonly Dictionary<int, CellTrigger> cellTriggers = new Dictionary<int, CellTrigger>();
        readonly Dictionary<CellPos, List<CellTrigger>> cellTriggerInfluence = new Dictionary<CellPos, List<CellTrigger>>();
        readonly Dictionary<int, ProximityTrigger> proximityTriggers = new Dictionary<int, ProximityTrigger>();

        int nextTriggerId;

        readonly CellLayer<InfluenceNode> influence;

        readonly Bin[] bins;
        readonly int rows, cols;

        /// <summary>
        /// 位置更新一次完成,以确保在一个Tick Time上的一致性
        /// </summary>
        readonly HashSet<Actor> addActorPosition = new HashSet<Actor>();
        readonly HashSet<Actor> removeActorPosition = new HashSet<Actor>();
        readonly Predicate<Actor> actorShouldBeRemoved;


        public ActorMap(World world,ActorMapInfo info)
        {
            this.info = info;
            this.map = world.Map;

            bins = new Bin[rows * cols];
            influence = new CellLayer<InfluenceNode>(world.Map);

            //缓存这个代理，不必重复分配
            actorShouldBeRemoved = removeActorPosition.Contains;
        }
        public void Tick(Actor self)
        {
            foreach(var bin in bins)
            {
                var removed = bin.Actors.RemoveAll(actorShouldBeRemoved);
                if (removed > 0)
                {
                    foreach (var t in bin.ProximityTriggers)
                        t.Dirty = true;
                }
            }
            removeActorPosition.Clear();

            foreach (var t in cellTriggers)
                t.Value.Tick(this);

            foreach (var t in proximityTriggers)
                t.Value.Tick(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cells"></param>
        /// <param name="onEntry"></param>
        /// <param name="OnExit"></param>
        /// <returns></returns>
        public int AddCellTrigger(CellPos[] cells,Action<Actor> onEntry,Action<Actor> OnExit)
        {
            var id = nextTriggerId++;
            var t = new CellTrigger(cells, onEntry, OnExit);
            cellTriggers.Add(id, t);

            foreach(var c in cells)
            {
                if (!influence.Contains(c))
                    continue;

                if (!cellTriggerInfluence.ContainsKey(c))
                    cellTriggerInfluence.Add(c, new List<CellTrigger>());

                cellTriggerInfluence[c].Add(t);
            }
            return id;
        }

        public void RemoveCellTrigger(int id)
        {
            CellTrigger trigger;
            if (!cellTriggers.TryGetValue(id, out trigger))
                return;
            foreach(var c in trigger.Footprint)
            {
                if (!cellTriggerInfluence.ContainsKey(c))
                    continue;

                cellTriggerInfluence[c].RemoveAll(t => t == trigger);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public IEnumerable<Actor> GetActorsAt(CellPos a)
        {
            var uv = a.ToMPos(map);
            if (!influence.Contains(uv))
                return Enumerable.Empty<Actor>();
            return new ActorsAtEnumerable(influence[uv]);
        }

        
    }
}