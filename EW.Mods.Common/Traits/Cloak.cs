using System;
using System.Collections.Generic;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class CloakInfo : UpgradableTraitInfo
    {
        public override object Create(ActorInitializer init)
        {
            return new Cloak(this);
        }
    }
    public class Cloak:UpgradableTrait<CloakInfo>
    {

        public Cloak(CloakInfo info) : base(info) { }
    }
}