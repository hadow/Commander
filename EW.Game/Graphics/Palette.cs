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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <param name="remapShadow"></param>
        void LoadFromStream(Stream s,int[] remapShadow)
        {

        }
        public uint this[int index]
        {
            get { return colors[index]; }
        }

        public void CopyToArray(Array destination,int destinationOffset)
        {

        }
    }

    public class MutablePalette : IPalette
    {
        readonly uint[] colors = new uint[Palette.Size];

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