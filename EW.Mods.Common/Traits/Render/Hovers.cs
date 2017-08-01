using System;
using System.Collections.Generic;

using EW.Traits;
namespace EW.Mods.Common.Traits
{


    public class HoversInfo : UpgradableTraitInfo
    {
        public override object Create(ActorInitializer init)
        {
            return new Hovers(this, init.Self);
        }
    }
    public class Hovers:UpgradableTrait<HoversInfo>
    {

        public Hovers(HoversInfo info,Actor self) : base(info) { }
    }
}