using System;
using EW.Activities;
using EW.Mods.Common.Traits;
namespace EW.Mods.Common.Activities
{
    public class FlyCircle:Activity
    {

        readonly Aircraft plane;
        readonly WDist cruiseAltitude;
        int remainingTicks;

        public FlyCircle(Actor self,int ticks = -1)
        {
            plane = self.Trait<Aircraft>();
            cruiseAltitude = plane.Info.CruiseAltitude;
            remainingTicks = ticks;

        }


        public override Activity Tick(Actor self)
        {
            if(plane.ForceLanding){

                Cancel(self);
                return NextActivity;
            }

            if (IsCanceled)
                return NextActivity;

            if (remainingTicks > 0)
                remainingTicks--;
            else if (remainingTicks == 0)
                return NextActivity;

            // We can't possibly turn this fast
            //我们不可能把这个速度变快
            var desiredFacing = plane.Facing + 64;
            Fly.FlyToward(self,plane,desiredFacing,cruiseAltitude);

            return this;

        }
    }
}
