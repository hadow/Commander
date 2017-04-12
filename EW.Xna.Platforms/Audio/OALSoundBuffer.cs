using System;
#if GLES
using OpenTK.Audio.OpenAL;
#endif

namespace EW.Xna.Platforms.Audio
{
    internal class OALSoundBuffer:IDisposable
    {
        int openAlDataBuffer;

        public int OpenALDataBuffer
        {
            get { return openAlDataBuffer; }
        }
        public OALSoundBuffer()
        {
            try
            {
                AL.GenBuffers(1, out openAlDataBuffer);
                ALHelper.CheckError("Failed to generate OpenAl data buffer");
            }
            catch(DllNotFoundException e)
            {

            }
        }
        public void Dispose() { }
    }
}