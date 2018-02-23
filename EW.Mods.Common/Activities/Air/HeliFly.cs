using System;
using EW.Activities;
using EW.Traits;
using EW.Mods.Common.Traits;

namespace EW.Mods.Common.Activities
{
    public class HeliFly:Activity
    {
        readonly Aircraft helicopter;
        readonly Target target;
        readonly WDist maxRange;
        readonly WDist minRange;

        bool playedSound;


        public HeliFly(Actor self,Target target)
        {
            helicopter = self.Trait<Aircraft>();
            this.target = target;
        }

        public HeliFly(Actor self,Target target,WDist minRange,WDist maxRange):this(self,target){
            this.minRange = minRange;
            this.maxRange = maxRange;
        }

        /// <summary>
        /// Adjusts the altitude.
        /// </summary>
        /// <returns><c>true</c>, if altitude was adjusted, <c>false</c> otherwise.</returns>
        /// <param name="self">Self.</param>
        /// <param name="helicopter">Helicopter.</param>
        /// <param name="targetAltitude">Target altitude.</param>
        public static bool AdjustAltitude(Actor self,Aircraft helicopter,WDist targetAltitude){

            targetAltitude = new WDist(helicopter.CenterPosition.Z) + targetAltitude - self.World.Map.DistanceAboveTerrain(helicopter.CenterPosition);

            var altitude = helicopter.CenterPosition.Z;

            if (altitude == targetAltitude.Length)
                return false;

            var delta = helicopter.Info.AltitudeVelocity.Length;

            var dz = (targetAltitude.Length - altitude).Clamp(-delta, delta);

            helicopter.SetPosition(self,helicopter.CenterPosition + new WVec(0,0,dz));

            return true;

        }

        public override Activity Tick(Actor self)
        {
            if(helicopter.ForceLanding){
                Cancel(self);
                return NextActivity;
            }

            if(IsCanceled ||!target.IsValidFor(self)){

                return NextActivity;
            }

            if(!playedSound && helicopter.Info.TakeoffSound != null && self.IsAtGroundLevel()){

                WarGame.Sound.Play(SoundType.World,helicopter.Info.TakeoffSound);
                playedSound = true;
            }

            if(AdjustAltitude(self,helicopter,helicopter.Info.CruiseAltitude)){
                return this;
            }

            var pos = target.CenterPosition;
            //Rotate towards the target

            var dist = pos - self.CenterPosition;
            var desiredFacing = dist.HorizontalLengthSquared != 0 ? dist.Yaw.Facing : helicopter.Facing;
            helicopter.Facing = Util.TickFacing(helicopter.Facing, desiredFacing, helicopter.TurnSpeed);

            var move = helicopter.FlyStep(desiredFacing);

            if(minRange.Length > 0 && target.IsInRange(helicopter.CenterPosition,minRange)){

                helicopter.SetPosition(self,helicopter.CenterPosition - move);
                return this;

            }

            if(maxRange.Length > 0 && target.IsInRange(helicopter.CenterPosition,maxRange)){
                return NextActivity;
            }

            if(dist.HorizontalLengthSquared < move.HorizontalLengthSquared){

                var targetAltitude = helicopter.CenterPosition.Z + helicopter.Info.CruiseAltitude.Length - self.World.Map.DistanceAboveTerrain(helicopter.CenterPosition).Length;
                helicopter.SetPosition(self,pos + new WVec(0,0,targetAltitude - pos.Z));
                return NextActivity;
            }

            helicopter.SetPosition(self,helicopter.CenterPosition + move);

            return this;



        }

        public override System.Collections.Generic.IEnumerable<Target> GetTargets(Actor self)
        {
            yield return target;
        }
    }


    public class HeliFlyAndLandWhenIdle:HeliFly{

        private readonly AircraftInfo info;

        public HeliFlyAndLandWhenIdle(Actor self,Target t,AircraftInfo info):base(self,t){
            this.info = info;
        }

        public override Activity Tick(Actor self)
        {
            var activity =  base.Tick(self);

            if(activity == null && !IsCanceled && info.LandWhenIdle){

                if (info.TurnToLand)
                    self.QueueActivity(new Turn(self, info.InitialFacing));
                self.QueueActivity(new HeliLand(self,true));
                activity = NextActivity;
            }

            return activity;

        }
    }
}
