using System;
using System.Collections.Generic;
using EW.Framework.Graphics;
using EW.Traits;
namespace EW.Graphics
{
    /// <summary>
    /// 硬件颜色寄存器
    /// </summary>
    public sealed class HardwarePalette
    {
        public ITexture Texture { get; private set; }
        public int Height { get; private set; }

        readonly Dictionary<string, ImmutablePalette> palettes = new Dictionary<string, ImmutablePalette>();

        readonly Dictionary<string, MutablePalette> modifiablePalettes = new Dictionary<string, MutablePalette>();

        readonly Dictionary<string, int> indices = new Dictionary<string, int>();

        readonly IReadOnlyDictionary<string, MutablePalette> readOnlyModifiablePalettes;

        byte[] buffer = new byte[0];

        public HardwarePalette()
        {
            Texture = WarGame.Renderer.Device.CreateTexture();
            readOnlyModifiablePalettes = modifiablePalettes.AsReadOnly();
        }

        public bool Contains(string name)
        {
            return modifiablePalettes.ContainsKey(name) || palettes.ContainsKey(name);
        }


        public void Initialize()
        {
            CopyModifiablePalettesToBuffer();
            CopyBufferToTexture();
        }

        /// <summary>
        /// 拷贝缓冲区字节数据添充贴图数据
        /// </summary>
        void CopyBufferToTexture()
        {
            Texture.SetData(buffer,Palette.Size,Height);
        }
        /// <summary>
        /// 
        /// </summary>
        void CopyModifiablePalettesToBuffer()
        {
            foreach(var kvp in modifiablePalettes)
            {
                CopyPaletteToBuffer(indices[kvp.Key], kvp.Value);
            }
        }

        /// <summary>
        /// 拷贝调色板至缓冲区
        /// </summary>
        /// <param name="index"></param>
        /// <param name="p"></param>
        void CopyPaletteToBuffer(int index,IPalette p)
        {
            p.CopyToArray(buffer, index * Palette.Size);
        }


        /// <summary>
        /// Implement dynamic hardware palette sizing
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
                //以2的增量增加调色板缓冲区和纹理，
                Height = Exts.NextPowerOf2(palettes.Count);
                Array.Resize(ref buffer, Height * Palette.Size * 4);
            }

            if (allowModifiers)
                modifiablePalettes.Add(name, new MutablePalette(p));
            else
                CopyPaletteToBuffer(index, p);
        }
        

        /// <summary>
        /// 替换调色板
        /// </summary>
        /// <param name="name"></param>
        /// <param name="p"></param>
        public void ReplacePalette(string name,IPalette p)
        {
            if (modifiablePalettes.ContainsKey(name))
                CopyPaletteToBuffer(indices[name], modifiablePalettes[name] = new MutablePalette(p));
            else if (palettes.ContainsKey(name))
                CopyPaletteToBuffer(indices[name], palettes[name] = new ImmutablePalette(p));
            else
                throw new InvalidOperationException("Palette '{0}' does not exist".F(name));

            CopyBufferToTexture();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="paletteMods"></param>
        public void ApplyModifiers(IEnumerable<IPaletteModifier> paletteMods)
        {
            foreach (var mod in paletteMods)
                mod.AdjustPalette(readOnlyModifiablePalettes);

            //Update our texture with the change
            CopyModifiablePalettesToBuffer();
            CopyBufferToTexture();

            //Reset modified palettes back to their original colors,ready for next time.
            //将修改后的调色板重置为原来的颜色，准备下次使用。
            foreach (var kvp in modifiablePalettes)
            {
                var originalPalette = palettes[kvp.Key];
                var modifiedPalette = kvp.Value;
                modifiedPalette.SetFromPalette(originalPalette);
            }
        }

        /// <summary>
        /// Get a palette index
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


        /// <summary>
        /// Get a palette.
        /// /summary>
        /// <param name="name"></param>
        /// <returns></returns>
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
            Texture.Dispose();
        }
    }
}