using System;
using System.IO;
using EW.Graphics;
using System.Drawing;
using EW.OpenGLES;
namespace EW.Mods.Common.SpriteLoaders
{
    public class TmpRALoader:ISpriteLoader
    {

        class TmpRAFrame : ISpriteFrame
        {
            public Size Size { get; private set; }
            public Size FrameSize { get; private set; }
            public Vector2 Offset { get { return Vector2.Zero; } }
            public byte[] Data { get; set; }
            public bool DisableExportPadding { get { return false; } }

            public TmpRAFrame(byte[] data, Size size)
            {
                FrameSize = size;
                Data = data;

                if (data == null)
                    Data = new byte[0];
                else
                    Size = size;
            }
        }

        bool IsTmpRA(Stream s)
        {
            var start = s.Position;

            s.Position += 20;
            var a = s.ReadUInt32();
            s.Position += 2;
            var b = s.ReadUInt16();

            s.Position = start;
            return a == 0 && b == 0x2c73;
        }

        TmpRAFrame[] ParseFrames(Stream s)
        {
            var start = s.Position;
            var width = s.ReadUInt16();
            var height = s.ReadUInt16();
            var size = new Size(width, height);

            s.Position += 12;
            var imgStart = s.ReadUInt32();
            s.Position += 8;
            var indexEnd = s.ReadInt32();
            s.Position += 4;
            var indexStart = s.ReadInt32();

            s.Position = indexStart;
            var count = indexEnd - indexStart;
            var tiles = new TmpRAFrame[count];

            var tilesIndex = 0;
            foreach (var b in s.ReadBytes(count))
            {
                if (b != 255)
                {
                    s.Position = imgStart + b * width * height;
                    tiles[tilesIndex++] = new TmpRAFrame(s.ReadBytes(width * height), size);
                }
                else
                    tiles[tilesIndex++] = new TmpRAFrame(null, size);
            }

            s.Position = start;
            return tiles;
        }

        public bool TryParseSprite(Stream s, out ISpriteFrame[] frames)
        {
            if (!IsTmpRA(s))
            {

                frames = null;
                return false;
            }
            frames = ParseFrames(s);
            return true;
        }
    }
}