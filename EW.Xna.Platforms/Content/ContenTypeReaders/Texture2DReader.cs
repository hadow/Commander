using System;
using EW.Xna.Platforms.Graphics;
namespace EW.Xna.Platforms.Content
{
    internal class Texture2DReader:ContentTypeReader<Texture2D>
    {

        protected internal override Texture2D Read(ContentReader input, Texture2D existingInstance)
        {
            Texture2D texture = null;

            var surfaceFormat = (SurfaceFormat)input.ReadInt32();

            int width = input.ReadInt32();
            int height = input.ReadInt32();

            int levelCount = input.ReadInt32();
            int levelCountOutput = levelCount;

            if(levelCount>1 && !input.GraphicsDevice.GraphicsCapabilities.SupportsNonPowerOfTwo)
            {
                levelCountOutput = 1;
                System.Diagnostics.Debug.WriteLine("Device does not support non Power of Two Textures.Skipping mipmaps.");
            }

            SurfaceFormat convertedFormat = surfaceFormat;
            switch (surfaceFormat)
            {
                case SurfaceFormat.Dxt1:
                case SurfaceFormat.Dxt1a:
                    if (!input.GraphicsDevice.GraphicsCapabilities.SupportsDxt1)
                        convertedFormat = SurfaceFormat.Color;
                    break;

            }

            texture = existingInstance ?? new Texture2D(input.GraphicsDevice, width, height, levelCountOutput > 1, convertedFormat);

            Threading.BlockOnUIThread(()=> {









            });


            return texture;
        }
    }
}