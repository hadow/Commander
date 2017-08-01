using System;
using System.Collections.Generic;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class WandersInfo : UpgradableTraitInfo
    {
        public override object Create(ActorInitializer init)
        {
            return new Wanders(init.Self, this);
        }
    }
    public class Wanders:UpgradableTrait<WandersInfo>
    {
        public Wanders(Actor self,WandersInfo info) : base(info)
        {

        }
    }
}