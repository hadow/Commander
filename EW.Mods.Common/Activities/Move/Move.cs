using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using EW.Mods.Common.Pathfinder;
using EW.Mods.Common.Traits;
using EW.Activities;
using EW.Traits;
using EW.Primitives;
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

        }

        public override bool Cancel(Actor self,bool keepQueue)
        {
            return base.Cancel(self,keepQueue);
        }

        public override void Queue(Activity activity)
        {
            base.Queue(activity);
        }


        public override Activity Tick(Actor self)
        {
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
                return ActivityUtils.SequenceActivities(new Turn(self, firstFacing), this);
            }
            else
            {
                mobile.SetLocation(mobile.FromCell, mobile.FromSubCell, nextCell.Value.First, nextCell.Value.Second);

                var map = self.World.Map;
                var from = (mobile.FromCell.Layer == 0 ? map.CenterOfCell(mobile.FromCell) :
                    self.World.GetCustomMovementLayers()[mobile.FromCell.Layer].CenterOfCell(mobile.FromCell)) + map.Grid.OffsetOfSubCell(mobile.FromSubCell);

                var to = Util.BetweenCells(self.World, mobile.FromCell, mobile.ToCell) +
                    (map.Grid.OffsetOfSubCell(mobile.FromSubCell) + map.Grid.OffsetOfSubCell(mobile.ToSubCell)) / 2;

                var move = new MoveFirstHalf(this, from, to, mobile.Facing, mobile.Facing, 0);
                return move;
            }
        }

        Pair<CPos,SubCell>? PopPath(Actor self)
        {
            if (path.Count == 0)
                return null;

            var nextCell = path[path.Count - 1];

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


        abstract class MovePart : Activity
        {
            protected readonly Move Move;
            protected readonly WPos From, To;
            protected readonly int FromFacing, ToFacing;

            protected readonly int MoveFractionTotal;

            protected int moveFraction;

            public MovePart(Move move,WPos from,WPos to,int fromFacing,int toFacing,int startingFraction)
            {
                Move = move;
                From = from;
                To = to;
                FromFacing = fromFacing;
                ToFacing = toFacing;
            }


            public override Activity Tick(Actor self)
            {
                throw new NotImplementedException();
            }
        }

        class MoveFirstHalf : MovePart
        {


            public MoveFirstHalf(Move move,WPos from,WPos to,int fromFacing,int toFacing,int startingFraction) : base(move, from, to, fromFacing, toFacing, startingFraction) { }
        }

        class MoveSecondHalf : MovePart
        {
            public MoveSecondHalf(Move move,WPos from,WPos to,int fromFacing,int toFacing,int startingFraction) : base(move, from, to, fromFacing, toFacing, startingFraction) { }
        }

    }
}