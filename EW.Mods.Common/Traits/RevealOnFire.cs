using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class RevealOnFireInfo:ConditionalTraitInfo
    {
        public override object Create(ActorInitializer init)
        {
            return new RevealOnFire(init.Self, this);
        }

    }

    public class RevealOnFire : ConditionalTrait<RevealOnFireInfo>
    {
        public RevealOnFire(Actor self,RevealOnFireInfo info) : base(info)
        {

        }

    }
}