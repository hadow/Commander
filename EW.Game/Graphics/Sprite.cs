using System;
using EW.Xna.Platforms.Graphics;
using EW.Xna.Platforms;
namespace EW.Graphics
{

    public enum TextureChannel :byte
    {
        Red = 0,
        Green = 1,
        Blue = 2,
        Alpha = 3
    }
    public class Sprite
    {
        public Sprite(Texture2D sheet,Rectangle bounds,float zRamp,Vector3 offset,TextureChannel channel,BlendMode blendMode = BlendMode.Alpha)
        {

        }
    }
}