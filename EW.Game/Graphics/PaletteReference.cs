using System;
using System.Collections.Generic;

namespace EW.Graphics
{
    /// <summary>
    /// 调色板引用
    /// </summary>
    public sealed class PaletteReference
    {
        readonly float index;
        readonly HardwarePalette hardwarePalette;

        public readonly string Name;
        
        public IPalette Palette { get; internal set; }

        public float TextureIndex { get { return index / hardwarePalette.Height; } }

        public float TextureMidIndex { get { return (index + 0.5f) / hardwarePalette.Height; } }

        public PaletteReference(string name,int index,IPalette palette,HardwarePalette hardwarePalette)
        {
            Name = name;
            Palette = palette;
            this.index = index;
            this.hardwarePalette = hardwarePalette;
        }

    }
}