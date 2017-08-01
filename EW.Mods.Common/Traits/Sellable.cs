using System;
using System.Collections.Generic;

using EW.Traits;
namespace EW.Mods.Common.Traits
{

    public class SellableInfo : UpgradableTraitInfo
    {
        public override object Create(ActorInitializer init)
        {
            return new Sellable(init.Self, this);
        }
    }


    public class Sellable:UpgradableTrait<SellableInfo>
    {

        public Sellable(Actor self,SellableInfo info) : base(info) { }

    }
}