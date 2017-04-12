using System;
using System.Collections.Generic;
#if GLES
using OpenTK.Audio.OpenAL;
#endif
namespace EW.Xna.Platforms.Audio
{
    public enum SoundState
    {
        Playing,
        Paused,
        Stopped,
    }
    public partial class SoundEffectInstance
    {
        internal OpenAlSoundController controller;
        internal SoundState SoundState = SoundState.Stopped;
        internal bool HasSourceId = false;

        internal int SourceId;

        private float _alVolume = 1;

        /// <summary>
        /// 
        /// </summary>
        private void PlatformPlay()
        {
            SourceId = 0;
            HasSourceId = false;
            SourceId = controller.ReserveSource();

            HasSourceId = true;

            int bufferId = _effect.SoundBuffer.OpenALDataBuffer;
            AL.Source(SourceId, ALSourcei.Buffer, bufferId);
            ALHelper.CheckError("Failed to bind buffer to source");

            if (!HasSourceId)
                return;

            AL.DistanceModel(ALDistanceModel.InverseDistanceClamped);
            ALHelper.CheckError("Failed set source distance");

            AL.Source(SourceId, ALSource3f.Position,_pan,0,0);
            ALHelper.CheckError("Failed to set Source pan.");

            AL.Source(SourceId, ALSourcef.Gain, _alVolume);
            ALHelper.CheckError("Failed to set source volume");

            AL.Source(SourceId, ALSourceb.Looping, IsLooped);
            ALHelper.CheckError("Failed to set source loop state");

            AL.Source(SourceId, ALSourcef.Pitch, ToAlPitch(_pitch));
            ALHelper.CheckError("Failed to set source pitch");

            AL.SourcePlay(SourceId);

            SoundState = SoundState.Playing;
        }


        private static float ToAlPitch(float pitch)
        {
            return (float)Math.Pow(2, pitch);
        }

        private void PlatformSetIsLooped(bool value)
        {
            _looped = value;

            if (HasSourceId)
            {
                AL.Source(SourceId, ALSourceb.Looping, _looped);
                ALHelper.CheckError("Failed to set source loop state.");
            }
        }


    }
}