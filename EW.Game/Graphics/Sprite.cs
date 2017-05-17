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

        public readonly Rectangle Bounds;
        public readonly Texture2D Sheet;
        public readonly BlendMode BlendMode;
        public readonly TextureChannel Channel;
        public readonly float ZRamp;
        public readonly Vector3 Size;
        public readonly Vector3 Offset;
        public readonly Vector3 FractionalOffset;
        public readonly float Top, Left, Bottom, Right;

        public Sprite(Texture2D sheet,Rectangle bounds,float zRamp,Vector3 offset,TextureChannel channel,BlendMode blendMode = BlendMode.Alpha)
        {

        }
    }
}