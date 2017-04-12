using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace EW.Xna.Platforms.Audio
{
    public sealed class NoAudioHardwareException:ExternalException
    {
        public NoAudioHardwareException(string msg) : base(msg) { }


        public NoAudioHardwareException(string msg,Exception innerException) : base(msg, innerException) { }
    }
}