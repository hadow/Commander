using System;
using System.Collections.Generic;
using System.Linq;
using EW.Traits;
using EW.Framework.Audio;
namespace EW.Mods.Common.Traits.Sound
{


    class AmbientSoundInfo : ConditionalTraitInfo
    {

        [FieldLoader.Require]
        public readonly string[] SoundFiles = null;


        public readonly int[] Delay = { 0 };

        public readonly int[] Interval = { 0 };

        public override object Create(ActorInitializer init)
        {
            return new AmbientSound(init.Self, this);
        }
    }


    class AmbientSound:ConditionalTrait<AmbientSoundInfo>,ITick,INotifyRemovedFromWorld
    {
        readonly bool loop;

        HashSet<ISound> currentSounds = new HashSet<ISound>();

        WPos cachedPosition;

        int delay;

        public AmbientSound(Actor self,AmbientSoundInfo info) : base(info)
        {

            delay = Util.RandomDelay(self.World, info.Delay);
            loop = Info.Interval.Length == 0 || (Info.Interval.Length == 1 && Info.Interval[0] == 0);
        }

        protected override void TraitEnabled(Actor self)
        {
            delay = Util.RandomDelay(self.World, Info.Delay);
        }


        protected override void TraitDisabled(Actor self)
        {
            StopSound();
        }

        void ITick.Tick(Actor self)
        {
            if (IsTraitDisabled)
                return;

            currentSounds.RemoveWhere(s => s == null || s.Complete);

            var pos = self.CenterPosition;
            if(pos != cachedPosition)
            {
                foreach (var s in currentSounds)
                    s.SetPosition(pos.ToVector3());
                cachedPosition = pos;
            }

            if (delay < 0)
                return;

            if (--delay > 0)
            {
                StartSound(self);
                if (!loop)
                    delay = Util.RandomDelay(self.World, Info.Interval);
            }
        }

        void INotifyRemovedFromWorld.RemovedFromWorld(Actor self)
        {
            StopSound();
        }


        void StartSound(Actor self)
        {
            var sound = Info.SoundFiles.RandomOrDefault(WarGame.CosmeticRandom);

            ISound s;
            if(self.OccupiesSpace != null)
            {
                cachedPosition = self.CenterPosition;
                s = loop ? WarGame.Sound.PlayLooped(SoundType.World, sound, cachedPosition) : WarGame.Sound.Play(SoundType.World, sound, self.CenterPosition);
            }
            else
            {
                s = loop ? WarGame.Sound.PlayLooped(SoundType.World, sound): WarGame.Sound.Play(SoundType.World,sound);
            }

            currentSounds.Add(s);

        }

        void StopSound()
        {
            foreach (var s in currentSounds)
                WarGame.Sound.StopSound(s);

            currentSounds.Clear();
        }

    }
}