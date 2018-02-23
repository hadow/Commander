using System;
using EW.Activities;
using EW.Mods.Common.Traits;
using EW.Traits;

namespace EW.Mods.Common.Activities
{
    public class Fly:Activity
    {
        readonly Aircraft plane;
        readonly Target target;
        readonly WDist maxRange;
        readonly WDist minRange;


        public Fly(Actor self,Target t)
        {
            plane = self.Trait<Aircraft>();
            target = t;
        }

        public Fly(Actor self,Target t,WDist minRange,WDist maxRange):this(self,t){

            this.maxRange = maxRange;
            this.minRange = minRange;
        }

        public override Activity Tick(Actor self)
        {
            if(plane.ForceLanding){
                Cancel(self);
                return NextActivity;
            }

            if (IsCanceled || !target.IsValidFor(self))
                return NextActivity;

            var insideMaxRange = maxRange.Length > 0 && target.IsInRange(plane.CenterPosition, maxRange);
            var insideMinrange = minRange.Length > 0 && target.IsInRange(plane.CenterPosition, minRange);

            if (insideMaxRange && !insideMinrange)
                return NextActivity;

            var d = target.CenterPosition - plane.CenterPosition;

            var move = plane.FlyStep(plane.Facing);

            if (d.HorizontalLengthSquared < move.HorizontalLengthSquared)
                return NextActivity;

            var desiredFacing = d.Yaw.Facing;
            var targetAltitude = plane.CenterPosition.Z + plane.Info.CruiseAltitude.Length - self.World.Map.DistanceAboveTerrain(plane.CenterPosition).Length;
            if (plane.CenterPosition.Z < targetAltitude)
                desiredFacing = plane.Facing;

            FlyToward(self,plane,desiredFacing,plane.Info.CruiseAltitude);

            return this;
        }


        public static void FlyToward(Actor self,Aircraft plane,int desiredFacing,WDist desiredAltitude)
        {

            desiredAltitude = new WDist(plane.CenterPosition.Z) + desiredAltitude - self.World.Map.DistanceAboveTerrain(plane.CenterPosition);

            var move = plane.FlyStep(plane.Facing);

            var altitude = plane.CenterPosition.Z;

            plane.Facing = Util.TickFacing(plane.Facing, desiredFacing, plane.TurnSpeed);

            if(altitude != desiredAltitude.Length){

                var delta = move.HorizontalLength * plane.Info.MaximumPitch.Tan() / 1024;
                var dz = (desiredAltitude.Length - altitude).Clamp(-delta, delta);
                move += new WVec(0, 0, dz);
            }

            plane.SetPosition(self,plane.CenterPosition+move);

        }

        public override System.Collections.Generic.IEnumerable<Target> GetTargets(Actor self)
        {
            yield return target;
        }


    }


    public class FlyAndContinueWithCirclesWhenIdle:Fly{

        public FlyAndContinueWithCirclesWhenIdle(Actor self,Target t):base(self,t){}

        public FlyAndContinueWithCirclesWhenIdle(Actor self,Target t,WDist minRange,WDist maxRange):base(self,t,minRange,maxRange){}


        public override Activity Tick(Actor self)
        {
            var activity = base.Tick(self);

            if(activity == null && !IsCanceled){

                self.QueueActivity(new FlyCircle(self));
                activity = NextActivity;
            }
            return activity;

        }
    }
}
