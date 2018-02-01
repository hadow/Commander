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
        List<CPos> path;            //移动路径
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

                return self.World.WorldActor.Trait<IPathFinder>()
                .FindUnitPath(mobile.ToCell, this.destination.Value, self, ignoreActor);
            };

            //Note:Will be recalculated from OnFirstRun if evaluateNearestMovableCell is true
            this.destination = destination;

            this.nearEnough = nearEnough;
            this.ignoreActor = ignoreActor;
            this.evaluateNearestMovableCell = evaluateNearestMovableCell;
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
            //通过消除路径来阻止他们永远移动
            if (path != null)
                path.Clear();


            //Remove queued activities
            if (!keepQueue && NextInQueue != null)
                NextInQueue = null;

            //In current implementation,ChildActivity can be Turn,MoveFirstHalf and MoveSecondHalf.
            //Turn may be interrupted freely while they are turning.                                //转弯时可能会自由中断
            //Unlike Turn,MoveFirstHalf and MoveSecondHalf are not Interruptable,but clearing the path guarantees that they will return as soon as possible,once the actor is back in a valid position.
            //与Turn 不同，MoveFirstHalf 和 MoveSecondHalf 是不可被中断的，但清除路径可以保证一旦单位回到有效的位置，他们将尽快返回
            //This means that it is safe to unconditionally return true,which avoids breaking parent activities that rely on cancellation succeeding (but not necessarily immediately)
            //这意味着无条件返回true 是安全的,避免了依靠取消成功(但不一定立即)
            ChildActivity.Cancel(self);

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

            //While carrying out one Move order,MoveSecondHalf finishes its work from time to time and returns null.//在执行一个Move命令时，MoveSecondHalf 不时完成其工作，并返回null.
            //That causes the ChildActivity to be null and makes us return to this part of code.                    //这会导致ChildActivity 为空，并使我们返回到这部分代码
            //If we only queue the activity and not run it ,units will lose one tick and pause briefly!             //如果我们只排队而不运行它，单位将会失去一个滴答，暂定一下！
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
                //Are we close enough?
                var cellRange = nearEnough.Length / 1024;
                if(!containsTemporaryBlocker && (mobile.ToCell - destination.Value).LengthSquard <= cellRange * cellRange)
                {
                    path.Clear();
                    return null;
                }

                //See if they will move
                if (!hasNotifiedBlocker)
                {
                    self.NotifyBlocker(nextCell);
                    hasNotifiedBlocker = true;
                }

                //Wait a bit to see if they leave
                if (!hasWaited)
                {
                    waitTicksRemaining = mobile.Info.WaitAverage + self.World.SharedRandom.Next(-mobile.Info.WaitSpread, mobile.Info.WaitSpread);
                    hasWaited = true;
                }

                if (--waitTicksRemaining >= 0)
                    return null;


                if(mobile.TicksBeforePathing > 0)
                {
                    mobile.TicksBeforePathing--;
                    return null;
                }

                //Calculate a new path
                mobile.RemoveInfluence();
                var newPath = EvalPath();
                mobile.AddInfluence();

                if (newPath.Count != 0)
                    path = newPath;

                return null;
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

            protected readonly int MoveFractionTotal;//移动至目标点的总计分
            protected int moveFraction;//移动分子

            public MovePart(Move move,WPos from,WPos to,int fromFacing,int toFacing,int startingFraction)
            {
                Move = move;
                From = from;
                To = to;
                FromFacing = fromFacing;
                ToFacing = toFacing;
                moveFraction = startingFraction;
                MoveFractionTotal = (to - from).Length;
                IsInterruptible = false;                //移动活动不可被中断

                //Calculate an elliptical arc that joins from and to
                //计算连接的椭圆弧
                var delta = Util.NormalizeFacing(fromFacing - toFacing);
                if(delta != 0&&delta != 128)
                {
                    //The center of rotation is where the normal vectors cross
                    //旋转的中心是法线向量交叉的地方
                    var u = new WVec(1024, 0, 0).Rotate(WRot.FromFacing(fromFacing));
                    var v = new WVec(1024, 0, 0).Rotate(WRot.FromFacing(toFacing));

                    var w = from - to;
                    var s = (v.Y * w.X - v.X * w.Y) * 1024 / (v.X * u.Y - v.Y * u.X);
                    var x = from.X + s * u.X / 1024;
                    var y = from.Y + s * u.Y / 1024;

                    ArcCenter = new WPos(x, y, 0);
                    ArcFromLength = (ArcCenter - from).HorizontalLength;
                    ArcFromAngle = (ArcCenter - from).Yaw;
                    ArcToLength = (ArcCenter - to).HorizontalLength;
                    ArcToAngle = (ArcCenter - to).Yaw;
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
                        var angle = WAngle.Lerp(ArcFromAngle, ArcToAngle, moveFraction, MoveFractionTotal);
                        var length = Int2.Lerp(ArcFromLength, ArcToLength, moveFraction, MoveFractionTotal);
                        var height = Int2.Lerp(From.Z, To.Z, moveFraction, MoveFractionTotal);
                        pos = ArcCenter + new WVec(0, length, length).Rotate(WRot.FromYaw(angle));

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