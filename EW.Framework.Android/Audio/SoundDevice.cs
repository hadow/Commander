using System;
using System.IO;
namespace EW.Framework.Audio
{
    public interface ISoundSource : IDisposable { }

    public interface ISound
    {
        float Volume { get; set; }

        float SeekPosition { get; }

        bool Complete { get; }

        void SetPosition(int x,int y,int z);

    }
    public class SoundDevice
    {
        public readonly string Device;
        public readonly string Label;

        public SoundDevice(string device, string label)
        {
            Device = device;
            Label = label;
        }

    }

    public interface ISoundEngine : IDisposable
    {
        //SoundDevice[] AvailableDevices();
        ISoundSource AddSoundSourceFromMemory(byte[] data, int channels, int sampleBits, int sampleRate);

        ISound Play2D(ISoundSource sound, bool loop, bool relative, Vector3 pos, float volume, bool attenuateVolume);

        ISound Play2DStream(Stream stream, int channels, int sampleBits, int sampleRate, bool loop, bool relative, Vector3 pos, float volume);

        float Volume { get; set; }

        void PauseSound(ISound sound, bool paused);

        void StopSound(ISound sound);

        void SetAllSoundsPaused(bool paused);

        void StopAllSounds();

        void SetListenerPosition(Vector3 position);

        void SetSoundVolume(float volume, ISound music, ISound video);




    }

}