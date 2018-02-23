using System;
using EW.Activities;
using EW.Mods.Common.Traits;
namespace EW.Mods.Common.Activities
{
    public class HeliLand:Activity
    {

        readonly Aircraft helicopter;//直升机
        readonly WDist landAltitude;
        readonly bool requireSpace;

        bool playedSound;



        public HeliLand(Actor self,bool requireSpace):this(self,requireSpace,self.Info.TraitInfo<AircraftInfo>().LandAltitude){}

        public HeliLand(Actor self,bool requireSpace,WDist landAltitude){
            this.requireSpace = requireSpace;
            this.landAltitude = landAltitude;
            helicopter = self.Trait<Aircraft>();
        }

        public override Activity Tick(Actor self)
        {
            if (IsCanceled)
                return NextActivity;

            if (requireSpace && !helicopter.CanLand(self.Location))
                return this;

            if(!playedSound && helicopter.Info.LandingSound != null && !self.IsAtGroundLevel()){

                WarGame.Sound.Play(SoundType.World,helicopter.Info.LandingSound);
                playedSound = true;

            }

            if (HeliFly.AdjustAltitude(self, helicopter, landAltitude))
                return this;

            return NextActivity;


        }
    }
}
