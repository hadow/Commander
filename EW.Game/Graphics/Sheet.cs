using System;
using System.Drawing;
using EW.Xna.Platforms;
using EW.Xna.Platforms.Graphics;
namespace EW.Graphics
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class Sheet:IDisposable
    {
        bool releaseBufferOnCommit;
        bool dirty;

        Texture2D texture;
        byte[] data;

        public readonly Size Size;
        public readonly SheetT Type;
        

        public Sheet(SheetT type,Size size)
        {
            Type = type;
            Size = size;
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
                texture.GetData(data);

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

        public Texture2D GetTexture()
        {
            if(texture == null)
            {
                texture = new Texture2D(GraphicsDeviceManager.M.GraphicsDevice, Size.Width, Size.Height);
                dirty = true;
            }

            if(data !=null && dirty)
            {
                texture.SetData(data);
                dirty = false;
                if (releaseBufferOnCommit)
                    data = null;
            }

            return texture;
        }

        /// <summary>
        /// 
        /// </summary>
        public void CommitBufferData()
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
            }
        }
    }
}