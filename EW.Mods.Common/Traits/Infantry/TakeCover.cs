using System;
using System.Collections.Generic;
using System.Linq;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    /// <summary>
    /// Make the unit go prone when under attack,in an attempt to reduce damage.
    /// 在受到攻击时使装置变得容易，以减少伤害
    /// </summary>
    public class TakeCoverInfo : TurretedInfo
    {
        [Desc("How long (in ticks) the actor remains prone.")]
        public readonly int ProneTime = 100;

        [Desc("Prone movement speed as a percentage of the normal speed.")]
        public readonly int SpeedModifier = 50;

        [FieldLoader.Require]
        [Desc("Damage types that trigger prone state. Defined on the warheads.")]
        public readonly HashSet<string> DamageTriggers = new HashSet<string>();

        [Desc("Damage modifiers for each damage type (defined on the warheads) while the unit is prone.")]
        public readonly Dictionary<string, int> DamageModifiers = new Dictionary<string, int>();

        public readonly WVec ProneOffset = new WVec(500, 0, 0);

        [SequenceReference(null, true)] public readonly string ProneSequencePrefix = "prone-";
    }
    
    public class TakeCover:Turreted,INotifyDamage,IDamageModifier,ISpeedModifier,ISync,IRenderInfantrySequenceModifier
    {
        readonly TakeCoverInfo info;
        [Sync] int remainingProneTime = 0;
        bool IsProne { get { return remainingProneTime > 0; } }

        bool IRenderInfantrySequenceModifier.IsModifyingSequence { get { return IsProne; } }
        string IRenderInfantrySequenceModifier.SequencePrefix { get { return info.ProneSequencePrefix; } }

        public TakeCover(ActorInitializer init, TakeCoverInfo info)
            : base(init, info)
        {
            this.info = info;
        }

        void INotifyDamage.Damaged(Actor self, AttackInfo e)
        {
            if (e.Damage.Value <= 0 || !e.Damage.DamageTypes.Overlaps(info.DamageTriggers))
                return;

            if (!IsProne)
                localOffset = info.ProneOffset;

            remainingProneTime = info.ProneTime;
        }

        protected override void Tick(Actor self)
        {
            base.Tick(self);

            if (IsProne && --remainingProneTime == 0)
                localOffset = WVec.Zero;
        }

        public override bool HasAchievedDesiredFacing
        {
            get { return true; }
        }

        int IDamageModifier.GetDamageModifier(Actor attacker, Damage damage)
        {
            if (!IsProne)
                return 100;

            if (damage.DamageTypes.Count == 0)
                return 100;

            var modifierPercentages = info.DamageModifiers.Where(x => damage.DamageTypes.Contains(x.Key)).Select(x => x.Value);
            return Util.ApplyPercentageModifiers(100, modifierPercentages);
        }

        int ISpeedModifier.GetSpeedModifier()
        {
            return IsProne ? info.SpeedModifier : 100;
        }

    }
}