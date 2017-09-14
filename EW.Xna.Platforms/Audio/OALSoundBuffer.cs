using System;
#if GLES
using OpenTK.Audio.OpenAL;
#endif

namespace EW.Xna.Platforms.Audio
{
    internal class OALSoundBuffer:IDisposable
    {
        int openAlDataBuffer;
        ALFormat openAlFormat;
        int dataSize;
        int sampleRate;
        bool _isDisposed;

        public double Duration { get; set; }
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataBuffer"></param>
        /// <param name="format"></param>
        /// <param name="size"></param>
        /// <param name="sampleRate"></param>
        /// <param name="alignment"></param>
        public void BindDataBuffer(byte[] dataBuffer,ALFormat format,int size,int sampleRate,int alignment = 0)
        {
            openAlFormat = format;
            dataSize = size;
            this.sampleRate = sampleRate;
            int unpackedSize = 0;

            AL.BufferData(openAlDataBuffer, openAlFormat, dataBuffer, size, this.sampleRate);
            ALHelper.CheckError("Failed to fill buffer");

            int bits, channels;

            AL.GetBuffer(openAlDataBuffer, ALGetBufferi.Bits, out bits);
            ALError alError = AL.GetError();
            if(alError != ALError.NoError)
            {
                Console.WriteLine("Failed to get bufer bits: {0},format = {1}, size={2}, sampleRate={3}", AL.GetErrorString(alError), format, size, sampleRate);
                Duration = -1;
            }
            else
            {
                AL.GetBuffer(openAlDataBuffer, ALGetBufferi.Channels, out channels);

                alError = AL.GetError();
                if(alError != ALError.NoError)
                {
                    Console.WriteLine("Failed to get bufer bits: {0},format = {1}, size={2}, sampleRate={3}", AL.GetErrorString(alError), format, size, sampleRate);
                    Duration = -1;
                }
                else
                {
                    AL.GetBuffer(openAlDataBuffer, ALGetBufferi.Size, out unpackedSize);
                    alError = AL.GetError();
                    if (alError != AL.GetError())
                    {
                        Console.WriteLine("Failed to get bufer bits: {0},format = {1}, size={2}, sampleRate={3}", AL.GetErrorString(alError), format, size, sampleRate);
                        Duration = -1;
                    }
                    else
                    {
                        Duration = (float)(unpackedSize / ((bits / 8) * channels)) / (float)sampleRate;
                    }
                }
            }
        }

        ~OALSoundBuffer()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    //Clean up managed objects

                }

                //Release unmanaged resources
                if (AL.IsBuffer(openAlDataBuffer))
                {
                    ALHelper.CheckError("Failed to fetch buffer state.");
                    AL.DeleteBuffers(1, ref openAlDataBuffer);
                    ALHelper.CheckError("Failed to delete buffer");
                }
                _isDisposed = true;
            }
        }
    }
}