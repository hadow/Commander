using System;
using System.Collections.Generic;
using EW.Traits;

namespace EW.Mods.Common.Traits
{
    public class InaccuracyMultiplierInfo : ConditionalTraitInfo
    {
        [FieldLoader.Require]
        public readonly int Modifier = 100;

        public override object Create(ActorInitializer init)
        {
            return new InaccuracyMultiplier(this);
        }
    }
    public class InaccuracyMultiplier:ConditionalTrait<InaccuracyMultiplierInfo>,IInaccuracyModifier
    {
        public InaccuracyMultiplier(InaccuracyMultiplierInfo info) : base(info) { }

        int IInaccuracyModifier.GetInaccuracyModifier(){
            return IsTraitDisabled ? 100 : Info.Modifier;
        }
    }
}