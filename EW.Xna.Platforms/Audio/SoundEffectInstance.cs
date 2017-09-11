using System;

namespace EW.Xna.Platforms.Audio
{
    /// <summary>
    /// 
    /// </summary>
    public partial class SoundEffectInstance:IDisposable
    {
        private bool _isDisposed = false;


        internal bool _isPooled = true;
        internal SoundEffect _effect;
        private bool _looped = false;
        private float _pan;
        private float _volume;
        private float _pitch;

        internal SoundEffectInstance(byte[] buffer,int sampleRate,int channels):this()
        {
            PlatformInitialize(buffer, sampleRate, channels);
        }

        internal SoundEffectInstance()
        {
            _pan = 0.0f;
            _volume = 1.0f;
            _pitch = 0.0f;
        }
        ~SoundEffectInstance()
        {
            Dispose(false);
        }

        public virtual bool IsLooped
        {
            get { return _isPooled; }
            set {
                PlatformSetIsLooped(value);
            }
        }




        public void Dispose()
        {
            
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
            }
        }
    }
}