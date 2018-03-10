using System;
using System.Collections.Generic;

using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class SoundOnDamageTransitionInfo : ITraitInfo
    {

        [Desc("Play a random sound from this list when damaged.")]
        public readonly string[] DamagedSounds = { };

        [Desc("Play a random sound from this list when destroyed.")]
        public readonly string[] DestroyedSounds = { };

        public object Create(ActorInitializer init) { return new SoundOnDamageTransition(this); }
    }


    public class SoundOnDamageTransition:INotifyDamageStateChanged
    {

        readonly SoundOnDamageTransitionInfo info;

        public SoundOnDamageTransition(SoundOnDamageTransitionInfo info)
        {
            this.info = info;
        }


        void INotifyDamageStateChanged.DamageStateChanged(Actor self, AttackInfo e)
        {
            var rand = WarGame.CosmeticRandom;

            if (e.DamageState == DamageState.Dead)
            {
                var sound = info.DestroyedSounds.RandomOrDefault(rand);
                WarGame.Sound.Play(SoundType.World, sound, self.CenterPosition);
            }
            else if (e.DamageState >= DamageState.Heavy && e.PreviousDamageState < DamageState.Heavy)
            {
                var sound = info.DamagedSounds.RandomOrDefault(rand);
                WarGame.Sound.Play(SoundType.World, sound, self.CenterPosition);
            }
        }

    }
}