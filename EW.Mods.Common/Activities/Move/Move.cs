using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using EW.Mods.Common.Pathfinder;
using EW.Mods.Common.Traits;
using EW.Activities;
using EW.Traits;
using EW.Primitives;
using EW.Framework;
namespace EW.Mods.Common.Activities
{
    /// <summary>
    /// 行进
    /// </summary>
    public class Move:Activity
    {
        static readonly List<CPos> NoPath = new List<CPos>();
        readonly Actor ignoreActor;
        readonly Func<List<CPos>> getPath;

        readonly WDist nearEnough;

        //For dealing with blockers
        bool hasWaited;
        bool hasNotifiedBlocker;
        int waitTicksRemaining;

        //To work around queued activity issues while minimizing changes to legacy behaviour
        //解决排队的活动问题，同时最大限度地减少对传统行为的更改
        bool evaluateNearestMovableCell;


        readonly Mobile mobile;
        List<CPos> path;
        CPos? destination;          //目的地

        //Scriptable move order
        //Ignores lane bias and nearby units.(忽略道路偏差和附近的单位)
        public Move(Actor self,CPos destination)
        {
            mobile = self.Trait<Mobile>();

            getPath = () =>
            {
                List<CPos> path;
                using(var search = PathSearch.FromPoint(self.World, mobile.Info, self, mobile.ToCell, destination, false).WithoutLaneBias())
                {
                    path = self.World.WorldActor.Trait<IPathFinder>().FindPath(search);
                }
                return path;

            };

            this.destination = destination;
            nearEnough = WDist.Zero;
            
        }
        public Move(Actor self,CPos destination,WDist nearEnough,Actor ignoreActor = null,bool evaluateNearestMovableCell = false)
        {
            mobile = self.Trait<Mobile>();

            getPath = () =>
            {
                if (!this.destination.HasValue)
                    return NoPath;

                return self.World.WorldActor.Trait<IPathFinder>().FindUnitPath(mobile.ToCell, this.destination.Value, self, ignoreActor);
            };

            //Note:Will be recalculated from OnFirstRun if evaluateNearestMovableCell is true
            this.destination = destination;

            this.nearEnough = nearEnough;
            this.ignoreActor = ignoreActor;
            this.evaluateNearestMovableCell = evaluateNearestMovableCell;
        }

        public Move(Actor self,CPos destination,Actor ignoredActor)
        {

        }

        public Move(Actor self,Func<List<CPos>> getPath)
        {
            mobile = self.Trait<Mobile>();

            this.getPath = getPath;

            destination = null;
            nearEnough = WDist.Zero;
        }

        public override bool Cancel(Actor self,bool keepQueue)
        {
            if (ChildActivity == null)
                return base.Cancel(self, keepQueue);

            //Although MoveFirstHalf and MoveSecondHalf can't be interrupted,
            //We prevent them from moving forever by removing the path.
            if (path != null)
                path.Clear();


            //Remove queued activities
            if (!keepQueue && NextInQueue != null)
                NextInQueue = null;

            ChildActivity.Cancel(self, false);

            return true;

        }

        protected override void OnFirstRun(Actor self)
        {
            if(evaluateNearestMovableCell && destination.HasValue)
            {
                var movableDestination = mobile.NearestMoveableCell(destination.Value);
                destination = mobile.CanEnterCell(movableDestination) ? movableDestination : (CPos?)null;
            }
        }


        public override Activity Tick(Actor self)
        {

            //ChildActivity is the top priority ,unlike other activities.
            //Even if this activity is canceled,we must let the child be run so that units
            //will not end up in an odd place.
            //ChildActivity的优先级最高，它不像其他的活动。即使这个活动被取消，我们也必须让它继续运转，这样单位不会最终停留在一个奇怪的地方。
            if (ChildActivity != null)
            {
                ChildActivity = ActivityUtils.RunActivity(self, ChildActivity);

                //Child activities such as Turn might have finished.
                //If we "return this" in this situationi,the unit loses one tick and pauses movement briefly.
                if (ChildActivity != null)
                    return this;
            }

            //If the actor is inside a tunnel then we must let them move
            //all the way through before moving to the next activity.
            if (IsCanceled && self.Location.Layer != CustomMovementLayerType.Tunnel)
                return NextActivity;

            if (mobile.IsTraitDisabled)
                return this;

            if (destination == mobile.ToCell)
                return NextActivity;

            if(path == null)
            {
                if(mobile.TicksBeforePathing>0)
                {
                    --mobile.TicksBeforePathing;
                    return this;
                }
                path = EvalPath();
                SanityCheckPath(mobile);
            }

            if(path.Count == 0)
            {
                destination = mobile.ToCell;
                return this;
            }

            destination = path[0];

            var nextCell = PopPath(self);
            if (nextCell == null)
                return this;

            var firstFacing = self.World.Map.FacingBetween(mobile.FromCell, nextCell.Value.First, mobile.Facing);

            if(firstFacing != mobile.Facing)
            {
                path.Add(nextCell.Value.First);
                //return ActivityUtils.SequenceActivities(new Turn(self, firstFacing), this);
                QueueChild(new Turn(self, firstFacing));
                return this;
            }
            mobile.SetLocation(mobile.FromCell, mobile.FromSubCell, nextCell.Value.First, nextCell.Value.Second);

            var map = self.World.Map;
            var from = (mobile.FromCell.Layer == 0 ? map.CenterOfCell(mobile.FromCell) :
                self.World.GetCustomMovementLayers()[mobile.FromCell.Layer].CenterOfCell(mobile.FromCell))
                + map.Grid.OffsetOfSubCell(mobile.FromSubCell);

            var to = Util.BetweenCells(self.World, mobile.FromCell, mobile.ToCell) +
                (map.Grid.OffsetOfSubCell(mobile.FromSubCell) + map.Grid.OffsetOfSubCell(mobile.ToSubCell)) / 2;

            //var move = new MoveFirstHalf(this, from, to, mobile.Facing, mobile.Facing, 0);
            QueueChild(new MoveFirstHalf(this, from, to, mobile.Facing, mobile.Facing, 0));

            ChildActivity = ActivityUtils.RunActivity(self, ChildActivity);
            return this;
        }

        Pair<CPos,SubCell>? PopPath(Actor self)
        {
            if (path.Count == 0)
                return null;

            var nextCell = path[path.Count - 1];

            var containsTemporaryBlocker = WorldUtils.ContainsTemporaryBlocker(self.World, nextCell, self);

            //Next cell in the move is blocked by another actor.
            if(containsTemporaryBlocker || !mobile.CanEnterCell(nextCell, ignoreActor, true))
            {

            }

            hasNotifiedBlocker = false;
            hasWaited = false;
            path.RemoveAt(path.Count - 1);
            var subCell = mobile.GetAvailableSubCell(nextCell, SubCell.Any, ignoreActor);
            return Pair.New(nextCell, subCell);
        }

        /// <summary>
        /// 计算路径
        /// </summary>
        /// <returns></returns>
        List<CPos> EvalPath()
        {
            var path = getPath().TakeWhile(a => a != mobile.ToCell).ToList();
            mobile.PathHash = HashList(path);
            return path;
        }

        [Conditional("SANITY_CHECKS")]  //健全性检查
        void SanityCheckPath(Mobile mobile)
        {
            if (path.Count == 0)
                return;
            var d = path[path.Count - 1] - mobile.ToCell;
            if (d.LengthSquard > 2)
                throw new InvalidOperationException("(Move) Sanity check failed");
        }


        static int HashList<T>(List<T> xs)
        {
            var hash = 0;
            var n = 0;
            foreach(var x in xs)
                hash += n++ * x.GetHashCode();
            return hash;
        }


        public override IEnumerable<Target> GetTargets(Actor self)
        {
            if (path != null)
                return Enumerable.Reverse(path).Select(c => Target.FromCell(self.World, c));
            if (destination != null)
                return new Target[] { Target.FromCell(self.World, destination.Value) };

            return Target.None;
        }


        abstract class MovePart : Activity
        {
            protected readonly Move Move;
            protected readonly WPos From, To;
            protected readonly int FromFacing, ToFacing;
            protected readonly bool EnableArc;
            protected readonly WPos ArcCenter;
            protected readonly int ArcFromLength;
            protected readonly WAngle ArcFromAngle;
            protected readonly int ArcToLength;
            protected readonly WAngle ArcToAngle;

            protected readonly int MoveFractionTotal;
            protected int moveFraction;

            public MovePart(Move move,WPos from,WPos to,int fromFacing,int toFacing,int startingFraction)
            {
                Move = move;
                From = from;
                To = to;
                FromFacing = fromFacing;
                ToFacing = toFacing;
                moveFraction = startingFraction;
                MoveFractionTotal = (to - from).Length;
                IsInterruptible = false;

                //
                var delta = Util.NormalizeFacing(fromFacing - toFacing);
                if(delta != 0&&delta != 128)
                {

                    EnableArc = true;
                }


            }


            public override Activity Tick(Actor self)
            {
                var ret = InnerTick(self, Move.mobile);
                Move.mobile.IsMoving = ret is MovePart;

                if (moveFraction > MoveFractionTotal)
                    moveFraction = MoveFractionTotal;

                UpdateCenterLocation(self, Move.mobile);

                if (ret == this)
                    return ret;

                Queue(ret);
                return NextActivity;
            }


            void UpdateCenterLocation(Actor self,Mobile mobile)
            {
                //Avoid division through zero
                if (MoveFractionTotal != 0)
                {
                    WPos pos = WPos.Zero;
                    if (EnableArc)
                    {

                    }
                    else
                        pos = WPos.Lerp(From, To, moveFraction, MoveFractionTotal);

                    mobile.SetVisualPosition(self, pos);

                }
                else
                    mobile.SetVisualPosition(self, To);

                if (moveFraction >= MoveFractionTotal)
                    mobile.Facing = ToFacing & 0xFF;
                else
                    mobile.Facing = Int2.Lerp(FromFacing, ToFacing, moveFraction, MoveFractionTotal) & 0xFF;
            }

            Activity InnerTick(Actor self,Mobile mobile)
            {
                moveFraction += mobile.MovementSpeedForCell(self, mobile.ToCell);
                if (moveFraction <= MoveFractionTotal)
                    return this;

                return OnComplete(self, mobile, Move);
            }



            protected abstract MovePart OnComplete(Actor self, Mobile mobile, Move parent);

            public override IEnumerable<Target> GetTargets(Actor self)
            {
                return Move.GetTargets(self);
            }

        }

        class MoveFirstHalf : MovePart
        {
            public MoveFirstHalf(Move move,WPos from,WPos to,int fromFacing,int toFacing,int startingFraction) 
                : base(move, from, to, fromFacing, toFacing, startingFraction) { }

            static bool IsTurn(Mobile mobile,CPos nextCell)
            {
                return nextCell - mobile.ToCell != mobile.ToCell - mobile.FromCell;
            }
            protected override MovePart OnComplete(Actor self, Mobile mobile, Move parent)
            {
                var map = self.World.Map;
                var fromSubcellOffset = map.Grid.OffsetOfSubCell(mobile.FromSubCell);
                var toSubcellOffset = map.Grid.OffsetOfSubCell(mobile.ToSubCell);


                if(!IsCanceled || self.Location.Layer == CustomMovementLayerType.Tunnel)
                {
                    var nextCell = parent.PopPath(self);
                    if(nextCell != null)
                    {
                        if (IsTurn(mobile, nextCell.Value.First))
                        {
                            var nextSubcellOffset = map.Grid.OffsetOfSubCell(nextCell.Value.Second);

                            var ret = new MoveFirstHalf(
                            Move,
                            Util.BetweenCells(self.World, mobile.FromCell, mobile.ToCell) + (fromSubcellOffset + toSubcellOffset) / 2,
                            Util.BetweenCells(self.World, mobile.ToCell, nextCell.Value.First) + (toSubcellOffset + nextSubcellOffset) / 2,
                            mobile.Facing,
                            Util.GetNearestFacing(mobile.Facing, map.FacingBetween(mobile.ToCell, nextCell.Value.First, mobile.Facing)),
                            moveFraction - MoveFractionTotal
                            );

                            mobile.FinishedMoving(self);
                            mobile.SetLocation(mobile.ToCell, mobile.ToSubCell, nextCell.Value.First, nextCell.Value.Second);
                            return ret;
                        }

                        parent.path.Add(nextCell.Value.First);
                    }
                }

                var toPos = mobile.ToCell.Layer == 0 ? map.CenterOfCell(mobile.ToCell) :
                    self.World.GetCustomMovementLayers()[mobile.ToCell.Layer].CenterOfCell(mobile.ToCell);

                var ret2 = new MoveSecondHalf(
                    Move,
                    Util.BetweenCells(self.World, mobile.FromCell, mobile.ToCell) + (fromSubcellOffset + toSubcellOffset) / 2,
                    toPos+toSubcellOffset,
                    mobile.Facing,
                    mobile.Facing,
                    moveFraction-MoveFractionTotal
                    );

                mobile.EnteringCell(self);
                mobile.SetLocation(mobile.ToCell, mobile.ToSubCell, mobile.ToCell, mobile.ToSubCell);
                return ret2;
            }
        }

        class MoveSecondHalf : MovePart
        {
            public MoveSecondHalf(Move move,WPos from,WPos to,int fromFacing,int toFacing,int startingFraction) : base(move, from, to, fromFacing, toFacing, startingFraction) { }

            protected override MovePart OnComplete(Actor self, Mobile mobile, Move parent)
            {
                mobile.SetPosition(self, mobile.ToCell);
                return null;
            }
        }

    }
}