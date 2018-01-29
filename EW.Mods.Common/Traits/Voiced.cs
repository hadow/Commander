using System;
using System.Collections.Generic;

using EW.Traits;
namespace EW.Mods.Common.Traits
{
    /// <summary>
    /// This actor has a voice.
    /// </summary>
    public class VoicedInfo : ITraitInfo
    {
        [FieldLoader.Require]
        [VoiceReference]
        public readonly string VoiceSet = null;

        /// <summary>
        /// Multiply volume with this factor.
        /// </summary>
        public readonly float Volume = 1f;

        public object Create(ActorInitializer init)
        {
            return new Voiced(init.Self,this);
        }
    }
    public class Voiced:IVoiced
    {

        public readonly VoicedInfo Info;

        public Voiced(Actor self,VoicedInfo info)
        {
            Info = info;
        }

        public string VoiceSet { get { return Info.VoiceSet; } }

        public bool PlayVoice(Actor self,string phrase,string variant)
        {
            if (phrase == null)
                return false;

            if (string.IsNullOrEmpty(Info.VoiceSet))
                return false;

            var type = Info.VoiceSet.ToLowerInvariant();
            var volume = Info.Volume;
            return WarGame.Sound.PlayPredefined(SoundType.World, self.World.Map.Rules, null, self, type, phrase, variant, true, WPos.Zero, volume, true);
        }


        public bool PlayVoiceLocal(Actor self,string phrase,string variant,float volume)
        {
            if (phrase == null)
                return false;

            if (string.IsNullOrEmpty(Info.VoiceSet))
                return false;

            var type = Info.VoiceSet.ToLowerInvariant();
            return WarGame.Sound.PlayPredefined(SoundType.World, self.World.Map.Rules, null, self, type, phrase, variant, false, self.CenterPosition, volume, true);
        }

        public bool HasVoice(Actor self,string voice)
        {
            if (string.IsNullOrEmpty(Info.VoiceSet))
                return false;

            var voices = self.World.Map.Rules.Voices[Info.VoiceSet.ToLowerInvariant()];
            return voices != null && voices.Voices.ContainsKey(voice);
        }


    }
}