using System;
using EW.Traits;

namespace EW.Mods.Common.Traits.Render
{


    public class WithReloadingSpriteTurretInfo : WithSpriteTurretInfo, Requires<AmmoPoolInfo>, Requires<ArmamentInfo>
    {
        public override object Create(ActorInitializer init)
        {
            return new WithReloadingSpriteTurret(init.Self, this);
        }
    }
    class WithReloadingSpriteTurret:WithSpriteTurret
    {

        public WithReloadingSpriteTurret(Actor self,WithReloadingSpriteTurretInfo info) : base(self, info) { }
    }
}