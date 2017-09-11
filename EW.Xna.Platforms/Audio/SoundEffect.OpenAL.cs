using System;
using OpenTK.Audio.OpenAL;
using System.IO;
namespace EW.Xna.Platforms.Audio
{
    public sealed partial class SoundEffect
    {

        internal OALSoundBuffer SoundBuffer;

        private void PlatformLoadAudioStream(Stream s,out TimeSpan duration)
        {
            byte[] buffer;
#if OPENAL && !(IOS)

            ALFormat format;
            int size;
            int freq;

            var stream = s;



#endif
        }
    }
}