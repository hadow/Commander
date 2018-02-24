using System;
using EW.Mods.Common.Traits;
using EW.Activities;
namespace EW.Mods.Common.Activities
{
    public class FlyTimed:Activity
    {

        readonly Aircraft plane;
        readonly WDist cruiseAltitude;

        int remainingTicks;

        public FlyTimed(int ticks,Actor self)
        {

            remainingTicks = ticks;
            plane = self.Trait<Aircraft>();
            cruiseAltitude = plane.Info.CruiseAltitude;
            
        }

        public override Activity Tick(Actor self)
        {
            if (plane.ForceLanding)
            {
                Cancel(self);
                return NextActivity;

            }

            if (IsCanceled || remainingTicks-- == 0)
                return NextActivity;

            Fly.FlyToward(self, plane, plane.Facing, cruiseAltitude);

            return this;
        }
    }
}
