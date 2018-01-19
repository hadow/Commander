using System;
using System.Drawing;
using System.Linq;
using System.Collections.Generic;
using EW.Activities;
using EW.Traits;
using EW.Mods.Common.Traits;
using EW.Mods.Common.Pathfinder;
namespace EW.Mods.Common.Activities
{
    public class MoveAdjacentTo:Activity
    {
        static readonly List<CPos> NoPath = new List<CPos>();

        protected readonly Mobile Mobile;
        readonly IPathFinder pathFinder;
        readonly DomainIndex domainIndex;
        readonly uint movementClass;

        Target target;

        protected Target Target
        {
            get { return target; }
            set
            {
                target = value;
            }
        }

        bool canHideUnderFog;

        protected CPos targetPosition;
        Activity inner;
        bool repath;

        public MoveAdjacentTo(Actor self,Target target)
        {
            Target = target;

            Mobile = self.Trait<Mobile>();
            pathFinder = self.World.WorldActor.Trait<IPathFinder>();
            domainIndex = self.World.WorldActor.Trait<DomainIndex>();
            movementClass = (uint)Mobile.Info.GetMovementClass(self.World.Map.Rules.TileSet);

            if (target.IsValidFor(self))
                targetPosition = self.World.Map.CellContaining(target.CenterPosition);

            repath = true;
        }

        public override Activity Tick(Actor self)
        {

            var targetIsValid = Target.IsValidFor(self);

            if(Target.Type == TargetT.Actor && canHideUnderFog && !Target.Actor.CanBeViewedByPlayer(self.Owner))
            {
                if (inner != null)
                    inner.Cancel(self);

                self.SetTargetLine(Target.FromCell(self.World, targetPosition), Color.Green);
                return ActivityUtils.RunActivity(self, new AttackMoveActivity(self, Mobile.MoveTo(targetPosition, 0)));
            }

            //Inner move order has completed
            if(inner == null)
            {
                if (IsCanceled || !repath || !targetIsValid)
                    return NextActivity;

                //Target has moved,and MoveAdjacentTo is still valid.
                inner = Mobile.MoveTo(() =>CalculatePathToTarget(self));
                repath = false;
            }

            if (targetIsValid)
            {
                //Check if the target has moved
                var oldTargetPosition = targetPosition;
                targetPosition = self.World.Map.CellContaining(Target.CenterPosition);

                var shouldStop = ShouldStop(self, oldTargetPosition);

                if(shouldStop || (!repath && ShouldRepath(self,oldTargetPosition)))
                {
                    //Finish moving into the next cell and then repath.
                    if (inner != null)
                        inner.Cancel(self);

                    repath = !shouldStop;
                }
            }
            else
            {
                //Target became invalid.Move to its last known position.
                Target = Target.FromCell(self.World, targetPosition);
            }
            //Ticks the inner move activity to actually move the actor.
            inner = ActivityUtils.RunActivity(self, inner);
            return this;

        }

        protected virtual bool ShouldStop(Actor self,CPos oldTargetPosition)
        {
            return false;
        }

        protected virtual bool ShouldRepath(Actor self,CPos oldTargetPosition)
        {
            return targetPosition != oldTargetPosition;
        }


        List<CPos> CalculatePathToTarget(Actor self)
        {
            var targetCells = CandidateMovementCells(self);
            var searchCells = new List<CPos>();
            var loc = self.Location;

            foreach (var cell in targetCells)
                if (domainIndex.IsPassable(loc, cell, Mobile.Info, movementClass) && Mobile.CanEnterCell(cell))
                    searchCells.Add(cell);

            if (!searchCells.Any())
                return NoPath;

            using (var fromSrc = PathSearch.FromPoints(self.World, Mobile.Info, self, searchCells, loc, true))
            using (var fromDest = PathSearch.FromPoint(self.World, Mobile.Info, self, loc, targetPosition, true).Reverse())
                return pathFinder.FindBidiPath(fromSrc, fromDest);
        }


        protected virtual IEnumerable<CPos> CandidateMovementCells(Actor self)
        {
            return Util.AdjacentCells(self.World, Target);
        }

        public override IEnumerable<Target> GetTargets(Actor self)
        {
            if (inner != null)
                return inner.GetTargets(self);
            return Target.None;
        }

        public override bool Cancel(Actor self, bool keepQueue = false)
        {
            if (!IsCanceled && inner != null && !inner.Cancel(self))
                return false;
            return base.Cancel(self, keepQueue);
        }



    }
}