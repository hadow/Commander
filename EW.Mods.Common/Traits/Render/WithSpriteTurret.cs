using System;
using EW.Traits;
namespace EW.Mods.Common.Traits.Render
{
    public class WithSpriteTurretInfo : ConditionalTraitInfo
    {
        public override object Create(ActorInitializer init)
        {
            return new WithSpriteTurret(init.Self, this);
        }
    }
    public class WithSpriteTurret:ConditionalTrait<WithSpriteTurretInfo>
    {
        public WithSpriteTurret(Actor self,WithSpriteTurretInfo info) : base(info) { }
    }
}