using System;
using EW.Traits;

namespace EW.Mods.Common.Traits
{
    class KillsSelfInfo:ConditionalTraitInfo
    {

        public override object Create(ActorInitializer init)
        {
            return new KillsSelf(init.Self, this);
        }

    }

    class KillsSelf : ConditionalTrait<KillsSelfInfo>
    {
        public KillsSelf(Actor self,KillsSelfInfo info) : base(info)
        {

        }
    }
}