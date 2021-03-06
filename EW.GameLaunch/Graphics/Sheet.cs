﻿using System;
using EW.Framework.Graphics;
using System.Drawing;
using Android.Graphics;
using System.IO;
using EW.Traits;
namespace EW.Graphics
{
    /// <summary>
    /// Sheet 包含一个可托管的纹理数据缓冲区，允许进行更新，而无需不断地获取并将数据设置到纹理内存中
    /// 这对于诸如SheetBuilder 之类的功能非常有用。这样可以对Sheet进行小的渐进式更改。
    /// 然后这些缓冲区通常很大，并且保持着live,因为sheets 被使用它们的sprite 引用，如果此缓冲区在不需要时显式为空，则GC可以回收它。
    /// 有时甚至不需要创建一个缓冲区，因为使用该Sheet 的对象只能直接在纹理上工作。
    /// </summary>
    public sealed class Sheet:IDisposable
    {
        bool releaseBufferOnCommit;
        bool dirty;

        ITexture texture;
        byte[] data;

        public readonly Size Size;
        public readonly SheetT Type;
        

        public Sheet(SheetT type,Size size)
        {
            Type = type;
            Size = size;
        }

        public Sheet(SheetT type,ITexture texture)
        {
            Type = type;
            this.texture = texture;
            Size = texture.Size;
        }

        public Sheet(SheetT type, Stream stream)
        {
            using (var bitmap = BitmapFactory.DecodeStream(stream, null,
              new BitmapFactory.Options
              {
                  InScaled = false,
                  InDither = false,
                  InJustDecodeBounds = false,
                  InPurgeable = true,
                  InInputShareable = true,
              }))
            {
                
                var width = bitmap.Width;
                var height = bitmap.Height;
                Size = new Size(width, height);
                data = new byte[4 * Size.Width * Size.Height];
                //int[] pixels = new int[width * height];
                //if((width != bitmap.Width ) || (height != bitmap.Height))
                //{

                //}
                //else
                //{
                //    bitmap.GetPixels(pixels, 0, width, 0, 0, width, height);

                //}

                //Util.FastCopyIntoChannel(new Sprite(this, bitmap.Bounds(), TextureChannel.Red), pixels.ToBytes());
                Util.FastCopyIntoSprite(new Sprite(this, bitmap.Bounds(), TextureChannel.Red), bitmap);
                bitmap.Recycle();

            }

            Type = type;
            ReleaseBuffer();
        }
        public bool Buffered { get { return data != null || texture == null; } }

        /// <summary>
        /// 获取纹理字节数据
        /// </summary>
        /// <returns></returns>
        public byte[] GetData()
        {
            CreateBuffer();
            return data;
        }

        public void CreateBuffer()
        {
            if (data != null)
                return;
            if (texture == null)
                data = new byte[4 * Size.Width * Size.Height];
            else
                data = texture.GetData();

            releaseBufferOnCommit = false;
        }

        /// <summary>
        /// 在Sheet 中生成缓冲区，按需创建字节缓冲区，
        /// 意味着新分配的Sprite不会浪费缓冲区上的存储直到有实际写入请求，这避免了新分配的Sprite 浪费缓存冲区中的内存，
        /// </summary>
        public void ReleaseBuffer()
        {
            if (!Buffered)
                return;
            dirty = true;
            releaseBufferOnCommit = true;
        }

        public ITexture GetTexture()
        {
            if(texture == null)
            {
                texture = WarGame.Renderer.Device.CreateTexture();
                dirty = true;
            }

            if(data !=null && dirty)
            {
                texture.SetData(data,Size.Width,Size.Height);
                dirty = false;
                if (releaseBufferOnCommit)
                    data = null;
            }

            return texture;
        }

        /// <summary>
        /// 
        /// </summary>
        public void CommitBufferedData()
        {
            if (!Buffered)
            {
                throw new InvalidOperationException(@"This sheet is unbuffered,You can't call CommitBufferedData on an unbuffered sheet.
                                                     If you need to completely replace the texture data you should set data into the texture directly.
                                                     If you need to make only small changed to the texture data consider creating a buffered sheet instead.
                            "

                    );

            }
            dirty = true;
        }
        

        public void Dispose()
        {
            if (texture != null)
            {
                texture.Dispose();
                texture = null;
            }
        }
    }
}