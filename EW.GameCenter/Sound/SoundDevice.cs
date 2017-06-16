using System;

namespace EW
{
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
		SoundDevice[] AvailableDevices();
		ISoundSource AddSoundSourceFromMemory(byte[] data, int channels, int sampleBits, int sampleRate);






	}



	public interface ISoundSource { }

	public interface ISound
	{
		float Volume { get; set; }
		float SeekPosition { get; }
		bool Playing { get; }
	}

}