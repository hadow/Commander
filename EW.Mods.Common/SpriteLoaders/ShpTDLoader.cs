using System;
using EW.Graphics;
using System.IO;
using System.Drawing;
using System.Linq;
using EW.Xna.Platforms;
using EW.Mods.Common.FileFormats;
namespace EW.Mods.Common.SpriteLoaders
{
    /// <summary>
    /// 
    /// </summary>
    public class ShpTDLoader:ISpriteLoader
    {
        static bool IsShpTD(Stream s)
        {
            var start = s.Position;

            //First word is the image count
            var imageCount = s.ReadUInt16();
            if(imageCount == 0)
            {
                s.Position = start;
                return false;
            }

            //Last offset should point to the end of file
            var finalOffset = start + 14 + 8 * imageCount;
            if(finalOffset > s.Length)
            {
                s.Position = start;
                return false;
            }
            s.Position = finalOffset;
            var eof = s.ReadUInt32();
            if(eof != s.Length)
            {
                s.Position = start;
                return false;
            }

            //Check the format flag on the first frame
            s.Position = start + 17;
            var b = s.ReadUInt8();

            s.Position = start;

            return b == 0x20 || b == 0x40 || b == 0x80;
        }

        public bool TryParseSprite(Stream s,out ISpriteFrame[] frames)
        {
            if (!IsShpTD(s))
            {
                frames = null;
                return false;
            }
            frames = new ShpTDSprite(s).Frames.ToArray();

            return true;
        }
    }

    public class ShpTDSprite
    {
        enum Format
        {
            XORPrev = 0x20,
            XORLCW = 0x40,
            LCW = 0x80
        }

        /// <summary>
        /// 图象头文件
        /// </summary>
        class ImageHeader : ISpriteFrame
        {
            ShpTDSprite reader;
            public Size Size { get { return reader.Size; } }
            public Size FrameSize { get { return reader.Size; } }

            public Vector2 Offset { get { return Vector2.Zero; } }

            public byte[] Data { get; set; }

            public bool DisableExportPadding { get { return false; } }

            public uint FileOffset;

            public Format Format;

            public uint RefOffset;

            public Format RefFormat;

            public ImageHeader RefImage;

            public ImageHeader(Stream s,ShpTDSprite reader)
            {
                this.reader = reader;

                var data = s.ReadUInt32();
                FileOffset = data & 0xffffff;
                Format = (Format)(data >> 24);

                RefOffset = s.ReadUInt16();
                RefFormat = (Format)s.ReadUInt16();
            }
        }

        public readonly Size Size;
        public IReadOnlyList<ISpriteFrame> Frames { get; private set; }

        int recurseDepth = 0;
        readonly int imageCount;

        readonly long shpBytesFileOffset;

        readonly byte[] shpBytes;


        public ShpTDSprite(Stream s)
        {
            imageCount = s.ReadUInt16();
            s.Position += 4;
            var width = s.ReadUInt16();
            var height = s.ReadUInt16();

            Size = new Size(width, height);

            s.Position += 4;
            var headers = new ImageHeader[imageCount];
            Frames = headers.AsReadOnly();
            for(var i = 0; i < headers.Length; i++)
            {
                headers[i] = new ImageHeader(s, this);
            }

            s.Position += 16;

            var offsets = headers.ToDictionary(h => h.FileOffset, h => h);

            for(var i = 0; i < imageCount; i++)
            {
                var h = headers[i];
                if (h.Format == Format.XORPrev)
                    h.RefImage = headers[i - 1];
                else if (h.Format == Format.XORLCW && !offsets.TryGetValue(h.RefOffset, out h.RefImage))
                    throw new InvalidOperationException("Reference doesn't point to image data {0} -> {1}".F(h.FileOffset, h.RefOffset));
            }

            shpBytesFileOffset = s.Position;
            shpBytes = s.ReadBytes((int)(s.Length - s.Position));

            foreach(var h in headers)
            {
                Decompress(h);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="h"></param>
        void Decompress(ImageHeader h)
        {
            if (h.Size.Width == 0 || h.Size.Height == 0)
                return;
            if (recurseDepth > imageCount)
                throw new InvalidOperationException("Format20/40 headers contain infinite loop");

            switch (h.Format)
            {
                case Format.XORPrev:
                case Format.XORLCW:
                    if(h.RefImage.Data == null)
                    {
                        ++recurseDepth;
                        Decompress(h.RefImage);
                        --recurseDepth;
                    }
                    h.Data = CopyImageData(h.RefImage.Data);
                    XORDeltaCompression.DecodeInto(shpBytes, h.Data, (int)(h.FileOffset - shpBytesFileOffset));
                    break;
                case Format.LCW:
                    var imageBytes = new byte[Size.Width * Size.Height];
                    LCWCompression.DecodeInto(shpBytes, imageBytes, (int)(h.FileOffset - shpBytesFileOffset));
                    h.Data = imageBytes;
                    break;
                default:
                    throw new InvalidDataException();
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseImage"></param>
        /// <returns></returns>
        byte[] CopyImageData(byte[] baseImage)
        {
            var imageData = new byte[Size.Width * Size.Height];
            Array.Copy(baseImage, imageData, imageData.Length);
            return imageData;
        }
    }
}