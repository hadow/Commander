using System;
using EW.Graphics;
using System.IO;
using System.Drawing;
using EW.Framework;
namespace EW.Mods.Common.SpriteLoaders
{
    public class TmpTDLoader:ISpriteLoader
    {

        class TmpTDFrame : ISpriteFrame
        {
            public Size Size { get; private set; }
            public Size FrameSize { get; private set; }
            public Vector2 Offset { get { return Vector2.Zero; } }
            public byte[] Data { get; set; }
            public bool DisableExportPadding { get { return false; } }

            public TmpTDFrame(byte[] data, Size size)
            {
                FrameSize = size;
                Data = data;

                if (data == null)
                    Data = new byte[0];
                else
                    Size = size;
            }
        }

        bool IsTmpTD(Stream s)
        {
            var start = s.Position;

            s.Position += 16;
            var a = s.ReadUInt32();
            var b = s.ReadUInt32();

            s.Position = start;
            return a == 0 && b == 0x0D1AFFFF;
        }

        TmpTDFrame[] ParseFrames(Stream s)
        {
            var start = s.Position;
            var width = s.ReadUInt16();
            var height = s.ReadUInt16();
            var size = new Size(width, height);

            s.Position += 8;
            var imgStart = s.ReadUInt32();
            s.Position += 8;
            var indexEnd = s.ReadInt32();
            var indexStart = s.ReadInt32();

            s.Position = indexStart;
            var count = indexEnd - indexStart;
            var tiles = new TmpTDFrame[count];
            var tilesIndex = 0;
            foreach (var b in s.ReadBytes(count))
            {
                if (b != 255)
                {
                    s.Position = imgStart + b * width * height;
                    tiles[tilesIndex++] = new TmpTDFrame(s.ReadBytes(width * height), size);
                }
                else
                    tiles[tilesIndex++] = new TmpTDFrame(null, size);
            }

            s.Position = start;
            return tiles;
        }

        public bool TryParseSprite(Stream s, out ISpriteFrame[] frames)
        {
            if (!IsTmpTD(s))
            {

                frames = null;
                return false;
            }
            frames = ParseFrames(s);
            return true;
        }
    }
}