using System;
using System.Linq;
using EW.Activities;
using EW.Mods.Common.Traits;
using EW.Traits;

namespace EW.Mods.Common.Activities
{
    public class FallToEarth:Activity
    {

        readonly Aircraft aircraft;
        readonly FallsToEarthInfo info;
        int acceleration = 0;
        int spin = 0;

        public FallToEarth(Actor self,FallsToEarthInfo info){

            this.info = info;
            IsInterruptible = false;
            aircraft = self.Trait<Aircraft>();
            if (info.Spins)
                acceleration = self.World.SharedRandom.Next(2) * 2 - 1;
            
        }
        public override Activity Tick(Actor self)
        {
            if(self.World.Map.DistanceAboveTerrain(self.CenterPosition).Length <=0){

                if(info.ExplosionWeapon != null)
                {
                    info.ExplosionWeapon.Impact(Target.FromPos(self.CenterPosition),self,Enumerable.Empty<int>());
                }

                self.Kill(self);
                return null;
            }


            if(info.Spins)
            {
                spin += acceleration;
                aircraft.Facing = (aircraft.Facing + spin) % 256;

            }

            var move = info.Moves ? aircraft.FlyStep(aircraft.Facing) : WVec.Zero;
            move -= new WVec(WDist.Zero, WDist.Zero, info.Velocity);
            aircraft.SetPosition(self,aircraft.CenterPosition + move);

            return this;

        }
    }
}
