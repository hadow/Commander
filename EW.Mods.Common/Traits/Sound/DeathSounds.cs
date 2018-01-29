using System;
using System.Collections.Generic;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class DeathSoundsInfo : ConditionalTraitInfo
    {
        /// <summary>
        /// Death notification voice.
        /// </summary>
        [VoiceReference]
        public readonly string Voice = "Die";

        public readonly float VolumeMultiplier = 1f;

        public readonly HashSet<string> DeathTypes = new HashSet<string>();
        public override object Create(ActorInitializer init)
        {
            return new DeathSounds(this);
        }
    }
    public class DeathSounds:ConditionalTrait<DeathSoundsInfo>,INotifyKilled
    {

        public DeathSounds(DeathSoundsInfo info) : base(info) { }

        public void Killed(Actor self,AttackInfo info)
        {
            if (IsTraitDisabled)
                return;

            if (Info.DeathTypes.Count == 0 || info.Damage.DamageTypes.Overlaps(Info.DeathTypes))
                self.PlayVoiceLocal(Info.Voice, Info.VolumeMultiplier);

        }


    }
}