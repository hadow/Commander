using System;
using System.Drawing;
using System.Linq;
using EW.Graphics;
namespace EW.Traits
{
    /// <summary>
    /// Add this to the Player actor definition.
    /// </summary>
    public class PlayerHighlightPaletteInfo : ITraitInfo
    {

        /// <summary>
        /// The prefix for the resulting player palettes
        /// </summary>
        [PaletteDefinition(true)]
        public readonly string BaseName = "highlight";

        public object Create(ActorInitializer init)
        {
            return new PlayerHighlightPalette(this);
        }
    }
    public class PlayerHighlightPalette:ILoadsPlayerPalettes
    {

        readonly PlayerHighlightPaletteInfo info;

        public PlayerHighlightPalette(PlayerHighlightPaletteInfo info)
        {
            this.info = info;
        }

        public void LoadPlayerPalettes(WorldRenderer wr,string playerName,HSLColor color,bool replaceExisting)
        {
            var argb = (uint)Color.FromArgb(128, color.RGB).ToArgb();
            var pal = new ImmutablePalette(Enumerable.Range(0, Palette.Size).Select(i => i == 0 ? 0 : argb));
            wr.AddPalette(info.BaseName + playerName, pal, false, replaceExisting);
        }
    }
}