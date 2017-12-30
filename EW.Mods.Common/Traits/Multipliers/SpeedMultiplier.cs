using System;
using System.Collections.Generic;

using EW.Traits;
namespace EW.Mods.Common.Traits
{


    /// <summary>
    /// Modifies the movement speed of this actor
    /// </summary>
    public class SpeedMultiplierInfo : ConditionalTraitInfo
    {

        [FieldLoader.Require]
        public readonly int Modifier = 100;//Percentage modifier to apply.
        public override object Create(ActorInitializer init)
        {
            return new SpeedMultiplier(this);
        }
    }
    public class SpeedMultiplier:ConditionalTrait<SpeedMultiplierInfo>,ISpeedModifier
    {
        public SpeedMultiplier(SpeedMultiplierInfo info) : base(info) { }

        public int GetSpeedModifier() { return IsTraitDisabled ? 100 : Info.Modifier; }
    }
}