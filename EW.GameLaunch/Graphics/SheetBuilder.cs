using System;
using System.Collections.Generic;
using System.Drawing;
using EW.OpenGLES;
using EW.OpenGLES.Graphics;
namespace EW.Graphics
{
    [Serializable]
    public class SheetOverflowException : Exception
    {
        public SheetOverflowException(string message):base(message)
        {

        }
    }

    /// <summary>
    /// The enum values indicate the number of channels used by the type
    /// They are not arbitrary IDs
    /// arbitrary 任意的
    /// </summary>
    public enum SheetT
    {
        Indexed = 1,
        BGRA = 4,
    }
    public sealed class SheetBuilder:IDisposable
    {

        public readonly SheetT Type;

        readonly Func<Sheet> allocateSheet;

        readonly List<Sheet> sheets = new List<Sheet>();
        int rowHeight = 0;
        System.Drawing.Point p;

        Sheet current;
        TextureChannel channel;
        
        public static Sheet AllocateSheet(SheetT type,int sheetSize)
        {
            return new Sheet(type, new Size(sheetSize,sheetSize));
        }
        
        public SheetBuilder(SheetT t):this(t,WarGame.Settings.Graphics.SheetSize){}

        public SheetBuilder(SheetT t,int sheetSize):this(t,()=>AllocateSheet(t,sheetSize)){}
        
        public SheetBuilder(SheetT t,Func<Sheet> allocateSheet)
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
                return new Sprite(current,EW.OpenGLES.Rectangle.Empty,0,spriteOffset,channel,BlendMode.Alpha);

            var rect = Allocate(size, zRamp, spriteOffset);
            Util.FastCopyIntoChannel(rect, src);
            current.CommitBufferedData();
            return rect;

        }


        public Sprite Allocate(Size imageSize)
        {
            return Allocate(imageSize, 0, Vector3.Zero);
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
            if(imageSize.Width + p.X > current.Size.Width)
            {
                p = new System.Drawing.Point(0, p.Y + rowHeight);
                rowHeight = imageSize.Height;
            }
            if (imageSize.Height > rowHeight)
                rowHeight = imageSize.Height;

            if(p.Y + imageSize.Height > current.Size.Height)
            {
                var next = NextChannel(channel);
                if (next == null)
                {
                    current.ReleaseBuffer();
                    current = allocateSheet();
                    sheets.Add(current);
                    channel = TextureChannel.Red;
                }
                else
                    channel = next.Value;

                rowHeight = imageSize.Height;
                p = new System.Drawing.Point(0, 0);
            }

            var rect = new Sprite(current, new EW.OpenGLES.Rectangle(p.X, p.Y, imageSize.Width, imageSize.Height), zRamp, spriteOffset, channel, BlendMode.Alpha);
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


        public Sheet Current { get { return current; } }


        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            foreach (var sheet in sheets)
                sheet.Dispose();

            sheets.Clear();
        }

        //protected override void Dispose(bool disposing)
        //{
        //    base.Dispose(disposing);
        //    foreach (var sheet in sheets)
        //        sheet.Dispose();

        //    sheets.Clear();
        //}











    }
}