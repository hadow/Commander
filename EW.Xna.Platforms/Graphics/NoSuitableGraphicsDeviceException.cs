using System;
using System.Runtime.Serialization;

namespace EW.Xna.Platforms.Graphics
{
    public sealed class NoSuitableGraphicsDeviceException:Exception
    {
        public NoSuitableGraphicsDeviceException() : base() { }

        public NoSuitableGraphicsDeviceException(string message) : base(message) { }

        public NoSuitableGraphicsDeviceException(string message,Exception inner) : base(message, inner) { }

    }
}