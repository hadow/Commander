using System;
using System.Collections.Generic;

using EW.Traits;
namespace EW.Mods.Common.Traits
{


    public class TargetableInfo : UpgradableTraitInfo
    {

        public readonly HashSet<string> TargetTypes = new HashSet<string>();

        public HashSet<string> GetTargetTypes() { return TargetTypes; }

        public bool RequiresForceFire = false;
        public override object Create(ActorInitializer init)
        {
            return new Targetable(init.Self, this);

        }
    }
    public class Targetable:UpgradableTrait<TargetableInfo>
    {
        public Targetable(Actor self,TargetableInfo info) : base(info) { }

    }
}