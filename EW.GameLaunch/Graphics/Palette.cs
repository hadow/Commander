using System;
using System.IO;
using System.Collections.Generic;
using EW.OpenGLES;
using System.Drawing;
namespace EW.Graphics
{
    public interface IPalette
    {
        uint this[int index] { get; }

        /// <summary>
        /// By allowing a palette to be copied to an array,
        /// a speedup can be gained in HardwarePalette since palettes can be block copied using native magic rather than having to copy them item by item
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="destinationOffset"></param>
        void CopyToArray(Array destination, int destinationOffset);
    }

    public interface IPaletteRemap { Color GetRemappedColor(Color original, int index); }

    public static class Palette
    {
        public const int Size = 256;


        public static Color GetColor(this IPalette palette,int index)
        {
            return Color.FromArgb((int)palette[index]);
        }
        public static IPalette AsReadOnly(this IPalette palette)
        {
            if (palette is ImmutablePalette)
                return palette;

            return new ReadOnlyPalette(palette);
        }

        /// <summary>
        /// 具有只读属性的Pal
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
    /// 不可变的调色板
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

        public ImmutablePalette(IPalette p,IPaletteRemap r):this(p)
        {
            for (var i = 0; i < Palette.Size; i++)
                colors[i] = (uint)r.GetRemappedColor(this.GetColor(i), i).ToArgb();
        }

        public ImmutablePalette(IPalette p)
        {
            for(int i = 0; i < Palette.Size; i++)
            {
                colors[i] = p[i];
            }
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
            colors[0] = 0;  //Convert black background to transparency
            foreach (var i in remapShadow)
                colors[i] = 140u << 24;


        }
        public uint this[int index]
        {
            get { return colors[index]; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="destinationOffset"></param>
        public void CopyToArray(Array destination,int destinationOffset)
        {
            //Buffer only affects arrays of primitive types;this class does not apply to objects.
            //Each primitive type is treated as a series of bytes without regard to any behaviour or limitation associated with the primitive type.
            //Buffer provides methods to copy bytes from one array of primitive types to another array of primitive types.get a byte from an array,set a byte in an arrary
            //and obtain the length of an array.
            //This class provides better performance for manipulating primitive types than similar methods in the System.Array class.
            Buffer.BlockCopy(colors, 0, destination, destinationOffset * 4, Palette.Size * 4);
        }
    }

    /// <summary>
    /// 可变的调色板
    /// </summary>
    public class MutablePalette : IPalette
    {
        readonly uint[] colors = new uint[Palette.Size];


        public MutablePalette(IPalette p)
        {
            SetFromPalette(p);
        }

        public void SetFromPalette(IPalette p)
        {
            p.CopyToArray(colors, 0);
        }

        public void CopyToArray(Array destination,int destinationOffset)
        {
            Buffer.BlockCopy(colors, 0, destination, destinationOffset * 4, Palette.Size * 4);
        }

        public uint this[int index]
        {
            get { return colors[index]; }
            set
            {
                colors[index] = value;
            }
        }

        public void SetColor(int index,Color color)
        {
            colors[index] = (uint)color.ToArgb();
        }
    }

}