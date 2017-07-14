using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EW.Xna.Platforms;
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
        /// <summary>
        /// Size of partition bins(cells)
        /// </summary>
        public readonly int BinSize = 10;

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
        /// 格子触发器
        /// </summary>
        class CellTrigger
        {
            public readonly CPos[] Footprint;
            public bool Dirty;

            readonly Action<Actor> onActorEntered;
            readonly Action<Actor> onActorExited;

            IEnumerable<Actor> currentActors = Enumerable.Empty<Actor>();
            
            public CellTrigger(CPos[] footprint,Action<Actor> onActorEntered,Action<Actor> onActorExited)
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
        /// 近距离触发器
        /// </summary>
        class ProximityTrigger:IDisposable
        {
            public WPos TopLeft { get; private set; }

            public WPos BottomRight { get; private set; }

            public bool Dirty;

            readonly Action<Actor> onActorEntered;
            readonly Action<Actor> onActorExited;

            WPos position;

            WDist rang;

            WDist vRange;

            IEnumerable<Actor> currentActors = Enumerable.Empty<Actor>();


            public ProximityTrigger(WPos pos,WDist range,WDist vRange,Action<Actor> onActorEntered,Action<Actor> onActorExited)
            {
                this.onActorEntered = onActorEntered;
                this.onActorExited = onActorExited;
            }

            public void Tick(ActorMap am)
            {
                if (!Dirty)
                    return;

                var oldActors = currentActors;
                var delta = new WVec(rang, rang, WDist.Zero);

                currentActors = am.ActorsInBox(position - delta, position + delta).Where(a=>(a.CenterPosition - position).HorizontalLengthSquared<rang.LengthSquared &&
                    (vRange.Length == 0 || (a.World.Map.DistanceAboveTerrain(a.CenterPosition).LengthSquared<=vRange.LengthSquared ))).ToList();

                var entered = currentActors.Except(oldActors);
                var exited = oldActors.Except(currentActors);

                if (onActorEntered != null)
                    foreach (var a in entered)
                        onActorEntered(a);

                if (onActorExited != null)
                    foreach (var a in exited)
                        onActorExited(a);


            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="newPos"></param>
            /// <param name="newRange"></param>
            /// <param name="newVRange"></param>
            public void Update(WPos newPos,WDist newRange,WDist newVRange)
            {
                position = newPos;
                rang = newRange;
                vRange = newVRange;

                var offset = new WVec(newRange, newRange, newVRange);

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

        readonly Dictionary<CPos, List<CellTrigger>> cellTriggerInfluence = new Dictionary<CPos, List<CellTrigger>>();

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
        public int AddCellTrigger(CPos[] cells,Action<Actor> onEntry,Action<Actor> OnExit)
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
        public IEnumerable<Actor> GetActorsAt(CPos a)
        {
            var uv = a.ToMPos(map);
            if (!influence.Contains(uv))
                return Enumerable.Empty<Actor>();
            return new ActorsAtEnumerable(influence[uv]);
        }

        public void AddInfluence(Actor self,IOccupySpace ios)
        {
            foreach(var c in ios.OccupiedCells())
            {
                var uv = c.First.ToMPos(map);
                if (!influence.Contains(uv))
                    continue;

                influence[uv] = new InfluenceNode
                {
                    Next = influence[uv],
                    SubCell = c.Second,
                    Actor = self
                };

                List<CellTrigger> triggers;
                if (cellTriggerInfluence.TryGetValue(c.First, out triggers))
                    foreach (var t in triggers)
                        t.Dirty = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="ios"></param>
        public void RemoveInfluence(Actor self,IOccupySpace ios)
        {
            foreach(var c in ios.OccupiedCells())
            {
                var uv = c.First.ToMPos(map);
                if (!influence.Contains(uv))
                    continue;

                var temp = influence[uv];
                RemoveInfluenceInner(ref temp, self);
                influence[uv] = temp;
                List<CellTrigger> triggers;
                if (cellTriggerInfluence.TryGetValue(c.First, out triggers))
                    foreach (var t in triggers)
                        t.Dirty = true;
            }
        }

        static void RemoveInfluenceInner(ref InfluenceNode influenceNode,Actor toRemove)
        {
            if (influenceNode == null)
                return;

            if (influenceNode.Actor == toRemove)
                influenceNode = influenceNode.Next;

            if (influenceNode != null)
                RemoveInfluenceInner(ref influenceNode.Next, toRemove);
            
        }

        public void AddPosition(Actor a,IOccupySpace ios)
        {
            UpdatePosition(a, ios);
        }

        public void RemovePosition(Actor a,IOccupySpace ios)
        {
            removeActorPosition.Add(a);
        }

        public void UpdatePosition(Actor a,IOccupySpace ios)
        {
            RemovePosition(a, ios);
            addActorPosition.Add(a);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public IEnumerable<Actor> ActorsInBox(WPos a,WPos b)
        {
            var left = Math.Min(a.X, b.X);
            var top = Math.Min(a.Y, b.Y);
            var right = Math.Max(a.X, b.X);
            var bottom = Math.Max(a.Y, b.Y);

            var region = BinRectangleCoveringWorldArea(left, top, right, bottom);

            var minCol = region.Left;
            var minRow = region.Top;
            var maxCol = region.Right;
            var maxRow = region.Bottom;

            for(var row = minRow; row <= maxRow; row++)
            {
                for(var col = minCol; col <= maxCol; col++)
                {
                    foreach(var actor in BinAt(row, col).Actors)
                    {
                        if (actor.IsInWorld)
                        {
                            var c = actor.CenterPosition;
                            if (left <= c.X && c.X <= right && top <= c.Y && c.Y <= bottom)
                                yield return actor;
                        }
                    }
                }
            }
            
        }

        Bin BinAt(int binRow,int binCol)
        {
            return bins[binRow * cols + binCol];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="worldLeft"></param>
        /// <param name="worldTop"></param>
        /// <param name="worldRight"></param>
        /// <param name="worldBottom"></param>
        /// <returns></returns>
        Rectangle BinRectangleCoveringWorldArea(int worldLeft,int worldTop,int worldRight,int worldBottom)
        {
            var minCol = WorldCoordtoBinIndex(worldLeft).Clamp(0,cols-1);
            var minRow = WorldCoordtoBinIndex(worldTop).Clamp(0, rows - 1);
            var maxCol = WorldCoordtoBinIndex(worldRight).Clamp(0, cols - 1);
            var maxRow = WorldCoordtoBinIndex(worldBottom).Clamp(0, rows - 1);
            return Rectangle.FromLTRB(minCol, minRow, maxCol, maxRow);
        }

        int WorldCoordtoBinIndex(int world)
        {
            return CellCoordToBinIndex(world / 1024);
        }

        int CellCoordToBinIndex(int cell)
        {
            return cell / info.BinSize;
        }
        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="checkTransient"></param>
        /// <returns></returns>
        public bool HasFreeSubCell(CPos cell,bool checkTransient = true)
        {
            return FreeSubCell(cell, SubCell.Any, checkTransient) != SubCell.Invalid;
        }

        public SubCell FreeSubCell(CPos cell,SubCell preferredSubCell = SubCell.Any,bool checkTransient = true)
        {

            return SubCell.Invalid;
        }
        
        public bool AnyActorsAt(CPos a)
        {
            var uv = a.ToMPos(map);
            if (!influence.Contains(uv))
                return false;

            return influence[uv] != null;
        }
    }
}