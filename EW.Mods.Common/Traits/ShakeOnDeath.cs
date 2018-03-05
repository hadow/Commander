using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    public class ShakeOnDeathInfo : ITraitInfo
    {
        public readonly int Intensity = 10;

        public object Create(ActorInitializer init) { return new ShakeOnDeath(this); }
    }
    public class ShakeOnDeath:INotifyKilled
    {


        readonly ShakeOnDeathInfo info;

        public ShakeOnDeath(ShakeOnDeathInfo info){
            this.info = info;
        }

        void INotifyKilled.Killed(Actor self,AttackInfo e){

            self.World.WorldActor.Trait<ScreenShaker>().AddEffect(info.Intensity,self.CenterPosition,1);
        }
    }
}