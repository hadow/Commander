using System;
using System.Collections.Generic;
using System.Linq;
using EW.Traits;
namespace EW.Mods.Common.Activities
{
    public class MoveWithinRange:MoveAdjacentTo
    {
        readonly WDist maxRange;
        readonly WDist minRange;

        public MoveWithinRange(Actor self,Target target,WDist minRange,WDist maxRange) : base(self, target)
        {
            this.minRange = minRange;
            this.maxRange = maxRange;
        }


        protected override bool ShouldStop(Actor self, CPos oldTargetPosition)
        {
            return AtCorrectRange(self.CenterPosition) && Mobile.CanInteractWithGroundLayer(self);
        }

        protected override bool ShouldRepath(Actor self, CPos oldTargetPosition)
        {
            return targetPosition != oldTargetPosition && (!AtCorrectRange(self.CenterPosition) || !Mobile.CanInteractWithGroundLayer(self));
        }

        protected override IEnumerable<CPos> CandidateMovementCells(Actor self)
        {
            var map = self.World.Map;
            var maxCells = (maxRange.Length + 1023) / 1024;
            var minCells = minRange.Length / 1024;

            return map.FindTilesInAnnulus(targetPosition, minCells, maxCells).Where(c => AtCorrectRange(map.CenterOfSubCell(c, Mobile.FromSubCell)));
        }

        bool AtCorrectRange(WPos origin)
        {
            return Target.IsInRange(origin, maxRange) && !Target.IsInRange(origin, minRange);
        }



    }
}