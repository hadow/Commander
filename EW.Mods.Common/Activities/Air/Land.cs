using System;
using EW.Activities;
using EW.Traits;
using EW.Mods.Common.Traits;
namespace EW.Mods.Common.Activities
{
    public class Land:Activity
    {
        readonly Target target;
        readonly Aircraft plane;
        public Land(Actor self,Target target)
        {
            this.target = target;
            plane = self.Trait<Aircraft>();
        }


        public override Activity Tick(Actor self)
        {
            if (!target.IsValidFor(self))
                Cancel(self);

            if (IsCanceled)
                return NextActivity;

            var d = target.CenterPosition - self.CenterPosition;

            var move = plane.FlyStep(plane.Facing);

            if(d.HorizontalLengthSquared < move.HorizontalLengthSquared)
            {
                plane.SetPosition(self, target.CenterPosition);
                return NextActivity;
            }

            Fly.FlyToward(self, plane, d.Yaw.Facing, new WDist(target.CenterPosition.Z));

            return this;
        }
    }
}
