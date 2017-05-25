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

        public PaletteReference(string name,int index,IPalette palette,HardwarePalette hardwarePalette)
        {
            Name = name;
            Palette = palette;
            this.index = index;
            this.hardwarePalette = hardwarePalette;
        }

    }
}