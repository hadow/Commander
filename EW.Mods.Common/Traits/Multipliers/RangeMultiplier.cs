using System;
namespace EW.Mods.Common.Traits
{

    public class RangeMultiplierInfo:ConditionalTraitInfo,IRangeModifierInfo{

        [FieldLoader.Require]
        public readonly int Modifier = 100;

        public override object Create(ActorInitializer init)
        {
            return new RangeMultiplier(this);
        }

        int IRangeModifierInfo.GetRangeModifierDefault(){

            return EnabledByDefault ? Modifier : 100;
        }
    }
    public class RangeMultiplier:ConditionalTrait<RangeMultiplierInfo>,IRangeModifier
    {
        public RangeMultiplier(RangeMultiplierInfo info):base(info)
        {
        }

        int IRangeModifier.GetRangeModifier(){
            return IsTraitDisabled ? 100 : Info.Modifier;
        }
    }
}
