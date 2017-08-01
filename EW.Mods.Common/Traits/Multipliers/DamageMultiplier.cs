using System;
using System.Collections.Generic;

using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class DamageMultiplierInfo : UpgradeMultiplierTraitInfo
    {
        public override object Create(ActorInitializer init)
        {
            return new DamageMultiplier(this, init.Self.Info.Name);
        }
    }

    public class DamageMultiplier:UpgradeMultiplierTrait,IDamageModifier
    {
        public DamageMultiplier(DamageMultiplierInfo info,string actorType) : base(info, "DamageMultiplier", actorType) { }

        public int GetDamageModifier(Actor attacker,IWarHead warhead) { return GetModifier(); }
    }
}