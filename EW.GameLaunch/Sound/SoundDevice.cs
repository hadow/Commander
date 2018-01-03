using System;
using System.IO;
namespace EW
{

    //public interface ISound
    //{
    //    float Volume { get; set; }

    //    float SeekPosition { get; }

    //    bool Complete { get; }

    //    void SetPosition(WPos pos);

    //}
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

    //public interface ISoundEngine : IDisposable
    //{
    //    SoundDevice[] AvailableDevices();
    //    ISoundSource AddSoundSourceFromMemory(byte[] data, int channels, int sampleBits, int sampleRate);

    //    ISound Play2D(ISoundSource sound, bool loop, bool relative, WPos pos, float volume, bool attenuateVolume);

    //    ISound Play2DStream(Stream stream, int channels, int sampleBits, int sampleRate, bool loop, bool relative, WPos pos, float volume);

    //    float Volume { get; set; }

    //    void PauseSound(ISound sound, bool paused);

    //    void StopSound(ISound sound);

    //    void SetAllSoundsPaused(bool paused);

    //    void StopAllSounds();

    //    void SetListenerPosition(WPos position);

    //    void SetSoundVolume(float volume, ISound music, ISound video);




    //}

}