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


        internal void PlatformInitialize(byte[] buffer,int sampleRate,int channels)
        {
            InitializeSound();
        }

        internal void InitializeSound()
        {
            controller = OpenAlSoundController.GetInstance;
        }



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

            //Distance Model
            AL.DistanceModel(ALDistanceModel.InverseDistanceClamped);
            ALHelper.CheckError("Failed set source distance");

            //Pan
            AL.Source(SourceId, ALSource3f.Position,_pan,0,0);
            ALHelper.CheckError("Failed to set Source pan.");

            //Volume
            AL.Source(SourceId, ALSourcef.Gain, _alVolume);
            ALHelper.CheckError("Failed to set source volume");


            //Looping
            AL.Source(SourceId, ALSourceb.Looping, IsLooped);
            ALHelper.CheckError("Failed to set source loop state");

            //Pitch
            AL.Source(SourceId, ALSourcef.Pitch, ToAlPitch(_pitch));
            ALHelper.CheckError("Failed to set source pitch");

            AL.SourcePlay(SourceId);
            ALHelper.CheckError("Failed to play source");
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

        private void PlatformPause()
        {

        }

        private void PlayformPlay()
        {

        }


    }
}