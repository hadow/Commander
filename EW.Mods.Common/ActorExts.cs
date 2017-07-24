using System;
using System.Collections.Generic;
using EW.Traits;

namespace EW.Mods.Common
{
    public static class ActorExts
    {
        public static bool IsAtGroundLevel(this Actor self)
        {
            if (self.IsDead)
                return false;

            if (self.OccupiesSpace == null)
                return false;

            if (!self.IsInWorld)
                return false;

            var map = self.World.Map;
            if (!map.Contains(self.Location))
                return false;

            return map.DistanceAboveTerrain(self.CenterPosition).Length == 0;
        }

        public static bool AppearsFriendlyTo(this Actor self,Actor toActor)
        {
            var stance = toActor.Owner.Stances[self.Owner];
            if(stance == Stance.Ally)
            {
                return true;
            }

            return stance == Stance.Ally;
        }
    }
}