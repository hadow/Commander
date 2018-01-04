using System.Linq;
using EW.Graphics;
using EW.Traits;
using System.Drawing;
namespace EW.Mods.Common.Traits
{
    /// <summary>
    /// Creates a single color palette without any base palette file.
    /// </summary>
    class PaletteFromRGBAInfo : ITraitInfo
    {
        public readonly string Name = null;

        public readonly string Tileset = null;

        public readonly int R = 0;

        public readonly int G = 0;

        public readonly int B = 0;

        public readonly int A = 255;

        public readonly bool AllowModifiers = true;

        public object Create(ActorInitializer init) { return new PaletteFromRGBA(init.World,this); }
    }
    class PaletteFromRGBA:ILoadsPalettes
    {
        readonly World world;
        readonly PaletteFromRGBAInfo info;

        public PaletteFromRGBA(World world,PaletteFromRGBAInfo info)
        {
            this.world = world;
            this.info = info;
        }

        public void LoadPalettes(WorldRenderer wr)
        {
            //Enable palette only for a specific tileset.
            if (info.Tileset != null && info.Tileset.ToLowerInvariant() != world.Map.Tileset.ToLowerInvariant())
                return;

            var a = info.A / 255f;
            var r = (int)(a * info.R + 0.5f).Clamp(0, 255);
            var g = (int)(a * info.G + 0.5f).Clamp(0, 255);
            var b = (int)(a * info.B + 0.5f).Clamp(0, 255);
            var c = (uint)Color.FromArgb(info.A, r, g, b).ToArgb();

            wr.AddPalette(info.Name, new ImmutablePalette(Enumerable.Range(0, Palette.Size).Select(i => (i == 0) ? 0 : c)), info.AllowModifiers);
                
        }
    }
}