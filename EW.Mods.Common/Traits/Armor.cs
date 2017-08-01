using System;
using System.Collections.Generic;
using EW.Traits;

namespace EW.Mods.Common.Traits
{

    public class ArmorInfo : UpgradableTraitInfo
    {
        public readonly string Type = null;
        public override object Create(ActorInitializer init)
        {
            return new Armor(init.Self, this);
        }
    }
    public class Armor:UpgradableTrait<ArmorInfo>
    {
        public Armor(Actor self,ArmorInfo info):base(info)
        {

        }
    }
}