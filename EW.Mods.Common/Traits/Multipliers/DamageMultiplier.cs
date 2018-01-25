using System;
using System.Collections.Generic;

using EW.Traits;
namespace EW.Mods.Common.Traits
{
    /// <summary>
    /// Modifies the damage applied to this actor,Use 0 to make actor invulnerable(无敌).
    /// </summary>
    public class DamageMultiplierInfo : ConditionalTraitInfo
    {

        [FieldLoader.Require]
        public readonly int Modifier = 100;
        public override object Create(ActorInitializer init)
        {
            return new DamageMultiplier(this);
        }
    }

    public class DamageMultiplier:ConditionalTrait<DamageMultiplierInfo>,IDamageModifier
    {
        public DamageMultiplier(DamageMultiplierInfo info) : base(info) { }

        int IDamageModifier.GetDamageModifier(Actor attacker,Damage damage){
            return IsTraitDisabled ? 100 : Info.Modifier;
        }

    }
}