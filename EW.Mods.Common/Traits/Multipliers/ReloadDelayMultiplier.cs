using System;
using System.Collections.Generic;
using EW.Traits;

namespace EW.Mods.Common.Traits
{
    public class ReloadDelayMultiplierInfo : ConditionalTraitInfo
    {
        [FieldLoader.Require]
        public readonly int Modifier = 100;
        public override object Create(ActorInitializer init)
        {
            return new ReloadDelayMultiplier(this);
        }
    }
    public class ReloadDelayMultiplier:ConditionalTrait<ReloadDelayMultiplierInfo>,IReloadModifier
    {
        public ReloadDelayMultiplier(ReloadDelayMultiplierInfo info) : base(info) { }

        int IReloadModifier.GetReloadModifier(){
            return IsTraitDisabled ? 100 : Info.Modifier;
        }
    }
}