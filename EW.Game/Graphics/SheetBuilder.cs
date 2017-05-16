using System;
using System.Collections.Generic;
using System.Drawing;
using EW.Xna.Platforms.Graphics;
using EW.Xna.Platforms;
namespace EW.Graphics
{
    public enum SheetT
    {
        Indexed = 1,
        BGRA = 2,
    }
    public sealed class SheetBuilder:IDisposable
    {

        public readonly SheetT Type;

        readonly Func<Texture2D> allocateSheet;

        readonly List<Texture2D> sheets = new List<Texture2D>();
        int rowHeight = 0;
        System.Drawing.Point p;

        Texture2D current;
        TextureChannel channel;
        
        public static Texture2D AllocateSheet(SheetT type,int sheetSize,GraphicsDevice device)
        {
            return new Texture2D(device, sheetSize, sheetSize);
        }
        
        public SheetBuilder(SheetT t,GraphicsDevice device):this(t,WarGame.Settings.Graphics.SheetSize,device){}

        public SheetBuilder(SheetT t,int sheetSize,GraphicsDevice device):this(t,()=>AllocateSheet(t,sheetSize,device)){}
        
        public SheetBuilder(SheetT t,Func<Texture2D> allocateSheet)
        {
            channel = TextureChannel.Red;
            Type = t;
            current = allocateSheet();
            sheets.Add(current);
            this.allocateSheet = allocateSheet;
        }

        public Sprite Add(ISpriteFrame frame)
        {
            return Add(frame.Data, frame.Size, 0, frame.Offset);
        }

        public Sprite Add(byte[] src,Size size)
        {
            return Add(src, size, 0, Vector3.Zero);
        }

        public Sprite Add(byte[] src,Size size,float zRamp,Vector3 spriteOffset)
        {
            if (size.Width == 0 || size.Height == 0)
                return null;

            var rect = Allocate(size, zRamp, spriteOffset);
            Util.FastCopyIntoChannel(rect, src);
            return rect;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="imageSize"></param>
        /// <param name="zRamp"></param>
        /// <param name="spriteOffset"></param>
        /// <returns></returns>
        public Sprite Allocate(Size imageSize,float zRamp,Vector3 spriteOffset)
        {
            if(imageSize.Width + p.X > current.Width)
            {
                p = new System.Drawing.Point(0, p.Y + rowHeight);
                rowHeight = imageSize.Height;
            }
            if (imageSize.Height > rowHeight)
                rowHeight = imageSize.Height;

            if(p.Y + imageSize.Height > current.Height)
            {
                var next = NextChannel(channel);
                if (next == null)
                {
                    current = allocateSheet();
                    sheets.Add(current);
                    channel = TextureChannel.Red;
                }
                else
                    channel = next.Value;

                rowHeight = imageSize.Height;
                p = new System.Drawing.Point(0, 0);
            }

            var rect = new Sprite(current, new Xna.Platforms.Rectangle(p.X, p.Y, imageSize.Width, imageSize.Height), zRamp, spriteOffset, channel, BlendMode.Alpha);
            p.X += imageSize.Width;

            return rect;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        TextureChannel? NextChannel(TextureChannel t)
        {
            var nextChannel = (int)t + (int)Type;
            if (nextChannel > (int)TextureChannel.Alpha)
                return null;
            return (TextureChannel)nextChannel;
        }


        public Texture2D Current { get { return current; } }

        public void Dispose()
        {
            foreach (var sheet in sheets)
                sheet.Dispose();

            sheets.Clear();
        }









    }
}