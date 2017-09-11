using System;
using System.IO;

namespace EW.Xna.Platforms.Audio
{
    public sealed partial class SoundEffect:IDisposable
    {

        #region Internal Audio Data

        private string _name = string.Empty;
        private bool _isDisposed = false;

        private readonly TimeSpan _duration;

        #endregion

        private SoundEffect(Stream stream)
        {

        }

        public void Dispose()
        {

        }
    }
}