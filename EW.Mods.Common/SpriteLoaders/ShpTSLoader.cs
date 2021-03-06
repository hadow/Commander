using System;
using System.IO;
using System.Drawing;
using EW.Graphics;
using EW.Framework;
using EW.Mods.Common.FileFormats;
namespace EW.Mods.Common.SpriteLoaders
{
    public class ShpTSLoader:ISpriteLoader
    {

        class ShpTSFrame : ISpriteFrame
        {
            public Size Size { get; private set; }
            public Size FrameSize { get; private set; }
            public Vector2 Offset { get; private set; }
            public byte[] Data { get; set; }
            public bool DisableExportPadding { get { return false; } }

            public readonly uint FileOffset;
            public readonly byte Format;

            public ShpTSFrame(Stream s, Size frameSize)
            {
                var x = s.ReadUInt16();
                var y = s.ReadUInt16();
                var width = s.ReadUInt16();
                var height = s.ReadUInt16();

                // Pad the dimensions to an even number to avoid issues with half-integer offsets
                var dataWidth = width;
                var dataHeight = height;
                if (dataWidth % 2 == 1)
                    dataWidth += 1;

                if (dataHeight % 2 == 1)
                    dataHeight += 1;

                Offset = new Vector2(x + (dataWidth - frameSize.Width) / 2, y + (dataHeight - frameSize.Height) / 2);
                Size = new Size(dataWidth, dataHeight);
                FrameSize = frameSize;

                Format = s.ReadUInt8();
                s.Position += 11;
                FileOffset = s.ReadUInt32();

                if (FileOffset == 0)
                    return;

                // Parse the frame data as we go (but remember to jump back to the header before returning!)
                var start = s.Position;
                s.Position = FileOffset;

                Data = new byte[dataWidth * dataHeight];

                if (Format == 3)
                {
                    // Format 3 provides RLE-zero compressed scanlines
                    for (var j = 0; j < height; j++)
                    {
                        var length = s.ReadUInt16() - 2;
                        RLEZerosCompression.DecodeInto(s.ReadBytes(length), Data, dataWidth * j);
                    }
                }
                else
                {
                    // Format 2 provides uncompressed length-prefixed scanlines
                    // Formats 1 and 0 provide an uncompressed full-width row
                    var length = Format == 2 ? s.ReadUInt16() - 2 : width;
                    for (var j = 0; j < height; j++)
                        s.ReadBytes(Data, dataWidth * j, length);
                }

                s.Position = start;
            }
        }

        bool IsShpTS(Stream s)
        {
            var start = s.Position;

            // First word is zero
            if (s.ReadUInt16() != 0)
            {
                s.Position = start;
                return false;
            }

            // Sanity Check the image count
            s.Position += 4;
            var imageCount = s.ReadUInt16();
            if (s.Position + 24 * imageCount > s.Length)
            {
                s.Position = start;
                return false;
            }

            // Check the size and format flag
            // Some files define bogus frames, so loop until we find a valid one
            s.Position += 4;
            ushort w, h, f = 0;
            byte type;
            do
            {
                w = s.ReadUInt16();
                h = s.ReadUInt16();
                type = s.ReadUInt8();
            }
            while (w == 0 && h == 0 && f++ < imageCount);

            s.Position = start;
            return type < 4;
        }

        ShpTSFrame[] ParseFrames(Stream s)
        {
            var start = s.Position;

            s.ReadUInt16();
            var width = s.ReadUInt16();
            var height = s.ReadUInt16();
            var size = new Size(width, height);
            var frameCount = s.ReadUInt16();

            var frames = new ShpTSFrame[frameCount];
            for (var i = 0; i < frames.Length; i++)
                frames[i] = new ShpTSFrame(s, size);

            s.Position = start;
            return frames;
        }


        public bool TryParseSprite(Stream s, out ISpriteFrame[] frames)
        {

            if (!IsShpTS(s))
            {
                frames = null;
                return false;
            }

            frames = ParseFrames(s);
            return true;
            

        }
    }
}