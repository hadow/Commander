using System;
namespace EW.Xna.Platforms
{
	[Flags]
	public enum DisplayOrientation
	{
		Default = 0,
		LandscapeLeft = 1,
		LandscapeRight = 2,
		Portrait = 4,
		PortraitDown = 8,
		Unknown=16


	}
}
