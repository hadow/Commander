using System;
using System.Collections.Generic;

namespace EW.Graphics
{
    public sealed class HardwarePalette:IDisposable
    {
        public int Height { get; private set; }
        readonly Dictionary<string, ImmutablePalette> palettes = new Dictionary<string, ImmutablePalette>();
        readonly Dictionary<string, MutablePalette> modifiablePalettes = new Dictionary<string, MutablePalette>();

        readonly Dictionary<string, int> indices = new Dictionary<string, int>();

        byte[] buffer = new byte[0];

        public bool Contains(string name)
        {
            return modifiablePalettes.ContainsKey(name) || palettes.ContainsKey(name);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="p"></param>
        /// <param name="allowModifiers"></param>
        public void AddPalette(string name,ImmutablePalette p,bool allowModifiers)
        {
            if (palettes.ContainsKey(name))
                throw new InvalidOperationException("Palette {0} has already benn defined.".F(name));

            int index = palettes.Count;
            indices.Add(name, index);
            palettes.Add(name, p);

            if (palettes.Count > Height)
            {
                Height = Exts.NextPowerOf2(palettes.Count);
                Array.Resize(ref buffer, Height * Palette.Size * 4);
            }

            if (allowModifiers)
                modifiablePalettes.Add(name, new MutablePalette(p));
            else
                CopyPaletteToBuffer(index, p);
        }

        void CopyPaletteToBuffer(int index,IPalette p)
        {
            p.CopyToArray(buffer, index * Palette.Size);
        }

        public void ReplacePalette(string name,IPalette p)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int GetPaletteIndex(string name)
        {
            int ret;
            if (!indices.TryGetValue(name, out ret))
                throw new InvalidOperationException("Palette '{0}' does not exist".F(name));
            return ret;
        }


        public IPalette GetPalette(string name)
        {
            MutablePalette mutable;
            if (modifiablePalettes.TryGetValue(name, out mutable))
                return mutable.AsReadOnly();

            ImmutablePalette immutable;
            if (palettes.TryGetValue(name, out immutable))
                return immutable;

            throw new InvalidOperationException("Palette '{0}' does not exist".F(name));

        }
        public void Dispose()
        {

        }
    }
}