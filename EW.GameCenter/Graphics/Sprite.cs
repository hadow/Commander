using System;
using EW.OpenGLES;
using EW.OpenGLES.Graphics;
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
        public readonly Sheet Sheet;
        public readonly BlendMode BlendMode;
        public readonly TextureChannel Channel;
        public readonly float ZRamp;
        public readonly Vector3 Size;
        public readonly Vector3 Offset;
        public readonly Vector3 FractionalOffset;
        public readonly float Top, Left, Bottom, Right;


        public Sprite(Sheet sheet,Rectangle bounds,TextureChannel channel) : this(sheet, bounds, 0, Vector2.Zero, channel) { }
        public Sprite(Sheet sheet,Rectangle bounds,float zRamp,Vector3 offset,TextureChannel channel,BlendMode blendMode = BlendMode.Alpha)
        {
            Sheet = sheet;
            Bounds = bounds;
            Offset = offset;
            ZRamp = zRamp;
            Channel = channel;
            Size = new Vector3(bounds.Width, bounds.Height, bounds.Height * ZRamp);
            BlendMode = blendMode;
            FractionalOffset = Size.Z != 0 ? offset / Size : new Vector3(offset.X / Size.X, offset.Y / Size.Y, 0);

            Left = (float)Math.Min(bounds.Left, bounds.Right) / sheet.Size.Width;
            Top = (float)Math.Min(bounds.Top, bounds.Bottom) / sheet.Size.Height;
            Right = (float)Math.Max(bounds.Left, bounds.Right) / sheet.Size.Width;
            Bottom = (float)Math.Max(bounds.Top, bounds.Bottom) / sheet.Size.Height;
        }
    }

    public class SpriteWithSecondaryData : Sprite
    {
        public readonly Rectangle SecondaryBounds;
        public readonly TextureChannel SecondaryChannel;
        public readonly float SecondaryTop, SecondaryLeft, SecondaryBottom, SecondaryRight;

        public SpriteWithSecondaryData(Sprite s,Rectangle secondaryBounds,TextureChannel secondaryChannel) : base(s.Sheet, s.Bounds, s.ZRamp, s.Offset, s.Channel, s.BlendMode)
        {
            SecondaryBounds = secondaryBounds;
            SecondaryChannel = secondaryChannel;

            SecondaryLeft = (float)Math.Min(secondaryBounds.Left, secondaryBounds.Right) / s.Sheet.Size.Width;
            SecondaryTop = (float)Math.Min(secondaryBounds.Top, secondaryBounds.Bottom) / s.Sheet.Size.Height;
            SecondaryRight = (float)Math.Max(secondaryBounds.Left, secondaryBounds.Right) / s.Sheet.Size.Width;
            SecondaryBottom = (float)Math.Max(secondaryBounds.Top, secondaryBounds.Bottom) / s.Sheet.Size.Height;
        }

    }
}