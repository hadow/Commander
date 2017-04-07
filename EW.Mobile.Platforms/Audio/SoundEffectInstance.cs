using System;

namespace EW.Mobile.Platforms.Audio
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
    }
}