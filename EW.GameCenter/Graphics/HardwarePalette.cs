using System;
using System.Collections.Generic;
using EW.Xna.Platforms.Graphics;
using EW.Xna.Platforms;
using EW.Traits;
namespace EW.Graphics
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class HardwarePalette:DrawableGameComponent
    {
        public Texture2D Texture { get; private set; }
        public int Height { get; private set; }
        readonly Dictionary<string, ImmutablePalette> palettes = new Dictionary<string, ImmutablePalette>();
        readonly Dictionary<string, MutablePalette> modifiablePalettes = new Dictionary<string, MutablePalette>();

        readonly Dictionary<string, int> indices = new Dictionary<string, int>();

        readonly IReadOnlyDictionary<string, MutablePalette> readOnlyModifiablePalettes;

        byte[] buffer = new byte[0];

        public HardwarePalette(Game game):base(game)
        {
            //Texture = new Texture2D(GraphicsDeviceManager.M.GraphicsDevice, Palette.Size, Height);

            readOnlyModifiablePalettes = modifiablePalettes.AsReadOnly();
        }

        public bool Contains(string name)
        {
            return modifiablePalettes.ContainsKey(name) || palettes.ContainsKey(name);
        }


        //public void Initialize()
        public override void Initialize()
        {
            if (Texture != null)
                Texture.Dispose();
            Texture = new Texture2D(this.GraphicsDevice, Palette.Size, Height);
            CopyModifiablePalettesToBuffer();
            CopyBufferToTexture();
        }

        /// <summary>
        /// 
        /// </summary>
        void CopyBufferToTexture()
        {
            Texture.SetData(buffer);
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
        /// 
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
        

        public void ReplacePalette(string name,IPalette p)
        {

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
            foreach(var kvp in modifiablePalettes)
            {
                var originalPalette = palettes[kvp.Key];
                var modifiedPalette = kvp.Value;
                modifiedPalette.SetFromPalette(originalPalette);
            }
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
        //public void Dispose()
        //{
        //    Texture.Dispose();
        //}

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Texture.Dispose();
        }
    }
}