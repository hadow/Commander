using System;
using System.IO;
using System.Collections.Generic;
namespace EW.Graphics
{
    public interface IPalette
    {
        uint this[int index] { get; }

        void CopyToArray(Array destination, int destinationOffset);
    }
    public static class Palette
    {
        public const int Size = 256;

        public static IPalette AsReadOnly(this IPalette palette)
        {
            if (palette is ImmutablePalette)
                return palette;

            return new ReadOnlyPalette(palette);
        }

        /// <summary>
        /// 
        /// </summary>
        class ReadOnlyPalette : IPalette
        {
            IPalette palette;

            public ReadOnlyPalette(IPalette palette) { this.palette = palette; }

            public uint this[int index] { get { return palette[index]; } }

            public void CopyToArray(Array destination,int destinationOffset)
            {
                palette.CopyToArray(destination, destinationOffset);
            }
        }

    }

    /// <summary>
    /// 
    /// </summary>
    public class ImmutablePalette : IPalette
    {
        readonly uint[] colors = new uint[Palette.Size];

        public ImmutablePalette(Stream s,int[] remapShadow)
        {
            LoadFromStream(s, remapShadow);
        }

        public ImmutablePalette(IEnumerable<uint> sourceColors)
        {
            var i = 0;
            foreach (var sourceColor in sourceColors)
                colors[i++] = sourceColor;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="remapShadow"></param>
        void LoadFromStream(Stream s,int[] remapShadow)
        {
            using(var reader = new BinaryReader(s))
            {
                for(var i = 0; i < Palette.Size; i++)
                {
                    var r = (byte)(reader.ReadByte() << 2);
                    var g = (byte)(reader.ReadByte() << 2);
                    var b = (byte)(reader.ReadByte() << 2);
                    colors[i] = (uint)((255 << 24) | (r << 16) | (g << 8) | b);
                }
            }
            colors[0] = 0;
            foreach (var i in remapShadow)
                colors[i] = 140u << 24;


        }
        public uint this[int index]
        {
            get { return colors[index]; }
        }

        public void CopyToArray(Array destination,int destinationOffset)
        {

        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class MutablePalette : IPalette
    {
        readonly uint[] colors = new uint[Palette.Size];


        public MutablePalette(IPalette p)
        {

        }

        public void SetFromPalette(IPalette p)
        {
            p.CopyToArray(colors, 0);
        }
        public void CopyToArray(Array destination,int destinationOffset)
        {

        }

        public uint this[int index]
        {
            get { return colors[index]; }
            set
            {
                colors[index] = value;
            }
        }
    }

}