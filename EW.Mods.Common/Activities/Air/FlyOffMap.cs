using System;
using EW.Activities;
using EW.Mods.Common.Traits;
namespace EW.Mods.Common.Activities
{
    public class FlyOffMap:Activity
    {
        readonly Aircraft plane;
        
        public FlyOffMap(Actor self)
        {
            plane = self.Trait<Aircraft>();
        }


        public override Activity Tick(Actor self)
        {

            if(plane.ForceLanding)
            {
                Cancel(self);
                return NextActivity;
            }

            if (IsCanceled || !self.World.Map.Contains(self.Location))
                return NextActivity;


            Fly.FlyToward(self, plane, plane.Facing, plane.Info.CruiseAltitude);

            return this;
        }
    }
}
