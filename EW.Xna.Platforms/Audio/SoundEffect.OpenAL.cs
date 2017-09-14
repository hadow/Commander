using System;
using OpenTK.Audio.OpenAL;
using System.IO;
namespace EW.Xna.Platforms.Audio
{
    public sealed partial class SoundEffect
    {

        internal OALSoundBuffer SoundBuffer;

//        private void PlatformLoadAudioStream(Stream s,out TimeSpan duration)
//        {
//            byte[] buffer;
//#if OPENAL && !(IOS)

//            ALFormat format;
//            int size;
//            int freq;

//            var stream = s;



//#endif
//        }

         internal ALFormat Format { get; set; }

        internal int Size { get; set; }

        internal float Rate { get; set; }

        private void PlatformLoadAudioStream(byte[] data,int channels,int sampleBits,int sampleRate)
        {

            SoundBuffer = new OALSoundBuffer();
            SoundBuffer.BindDataBuffer(data, Format, Size, (int)Rate);
        }
    }
}