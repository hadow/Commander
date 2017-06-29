using System;
using System.Collections.Generic;
using EW.Xna.Platforms.Graphics;
using EW.Xna.Platforms;
namespace EW.Graphics
{
    public static class Util
    {

        static readonly int[] ChannelMasks = { 2, 1, 0, 3 };
        static readonly float[] ChannelSelect = { 0.2f, 0.4f, 0.6f, 0.8f };
        /// <summary>
        /// 
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="o"></param>
        /// <param name="r"></param>
        /// <param name="paletteTextureIndex"></param>
        /// <param name="nv"></param>
        /// <param name="size"></param>
        public static void FastCreateQuad(Vertex[] vertices,Vector3 o,Sprite r,float paletteTextureIndex,int nv,Vector3 size)
        {
            var b = new Vector3(o.X + size.X, o.Y, o.Z);
            var c = new Vector3(o.X + size.X, o.Y + size.Y, o.Z + size.Z);
            var d = new Vector3(o.X, o.Y + size.Y, o.Z + size.Z);
            FastCreateQuad(vertices, o, b, c, d, r, paletteTextureIndex, nv);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        /// <param name="r"></param>
        /// <param name="paletteTextureIndex"></param>
        /// <param name="nv"></param>
        public static void FastCreateQuad(Vertex[] vertices,Vector3 a,Vector3 b,Vector3 c,Vector3 d,Sprite r,float paletteTextureIndex,int nv)
        {
            float sl = 0;
            float st = 0;
            float sr = 0;
            float sb = 0;

            var attribC = ChannelSelect[(int)r.Channel];

            var ss = r as SpriteWithSecondaryData;
            if (ss != null)
            {
                sl = ss.SecondaryLeft;
                st = ss.SecondaryTop;
                sr = ss.SecondaryRight;
                sb = ss.SecondaryBottom;

                attribC = -(attribC + ChannelSelect[(int)ss.SecondaryChannel] / 10);
            }

            //vertices[nv] = new Vertex { Position = a, TextureCoordinate = new Vector2(r.Left,r.Top),UV = new Vector2(sl,st),Palette = paletteTextureIndex,C = attribC };
            //vertices[nv + 1] = new Vertex { Position = b, TextureCoordinate = new Vector2(r.Right, r.Top), UV = new Vector2(sr, st), Palette = paletteTextureIndex, C = attribC };
            //vertices[nv + 2] = new Vertex { Position = c, TextureCoordinate = new Vector2(r.Right, r.Bottom), UV = new Vector2(sr, sb), Palette = paletteTextureIndex, C = attribC };
            //vertices[nv + 3] = new Vertex { Position = c, TextureCoordinate = new Vector2(r.Right, r.Bottom), UV = new Vector2(sr, sb), Palette = paletteTextureIndex, C = attribC };
            //vertices[nv + 4] = new Vertex { Position = d, TextureCoordinate = new Vector2(r.Left, r.Bottom), UV = new Vector2(sl, sb), Palette = paletteTextureIndex, C = attribC };
            //vertices[nv + 5] = new Vertex { Position = a, TextureCoordinate = new Vector2(r.Left, r.Top), UV = new Vector2(sl, st), Palette = paletteTextureIndex, C = attribC };

            vertices[nv] = new Vertex(a, new Vector2(r.Left, r.Top), new Vector2(sl, st), paletteTextureIndex, attribC);
            vertices[nv + 1] = new Vertex(b, new Vector2(r.Right, r.Top), new Vector2(sr, st), paletteTextureIndex, attribC);
            vertices[nv + 2] = new Vertex(c, new Vector2(r.Right, r.Bottom), new Vector2(sr, sb), paletteTextureIndex, attribC);
            vertices[nv + 3] = new Vertex(c, new Vector2(r.Right, r.Bottom), new Vector2(sr, sb), paletteTextureIndex, attribC);
            vertices[nv + 4] = new Vertex(d, new Vector2(r.Left, r.Bottom), new Vector2(sl, sb), paletteTextureIndex, attribC);
            vertices[nv + 5] = new Vertex(a, new Vector2(r.Left, r.Top), new Vector2(sl, st), paletteTextureIndex, attribC);

        }




        /// <summary>
        /// 
        /// </summary>
        /// <param name="dest"></param>
        /// <param name="src"></param>
        public static void FastCopyIntoChannel(Sprite dest,byte[] src)
        {
            var data = dest.Sheet.GetData();
            
            var srcStride = dest.Bounds.Width;
            var destStride = dest.Sheet.Size.Width * 4;
            var destOffset = destStride * dest.Bounds.Top + dest.Bounds.Left * 4 + ChannelMasks[(int)dest.Channel];
            var destSkip = destStride - 4 * srcStride;
            var height = dest.Bounds.Height;

            var srcOffset = 0;
            for(var j = 0; j < height; j++)
            {
                for(var i = 0; i < srcStride; i++, srcOffset++)
                {
                    data[destOffset] = src[srcOffset];
                    destOffset += 4;
                }
                destOffset += destSkip;
            }

        }

    }
}