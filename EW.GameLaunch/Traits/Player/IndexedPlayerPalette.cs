using System.Collections.Generic;
using System.Drawing;
using System.IO;
using EW.Graphics;

namespace EW.Traits
{

    public class IndexedPlayerPaletteInfo : ITraitInfo,IRulesetLoaded
    {

        [PaletteReference]
        public readonly string BasePalette = null;

        [PaletteDefinition(true)]
        public readonly string BaseName = "player";
        /// <summary>
        /// Remap these indices to player colors.
        /// </summary>
        public readonly int[] RemapIndex = { };

        /// <summary>
        /// Allow palette modifiers to change the palette.
        /// </summary>
        public readonly bool AllowModifiers = true;

        public readonly Dictionary<string, int[]> PlayerIndex;

        public object Create(ActorInitializer init) { return new IndexedPlayerPalette(this); }


        public void RulesetLoaded(Ruleset rules,ActorInfo ai)
        {
            foreach (var p in PlayerIndex)
                if (p.Value.Length != RemapIndex.Length)
                    throw new YamlException("PlayerIndex for player '{0} length does not match RemapIndex!".F(p.Key));
        }
    }
    class IndexedPlayerPalette:ILoadsPlayerPalettes
    {

        readonly IndexedPlayerPaletteInfo info;

        public IndexedPlayerPalette(IndexedPlayerPaletteInfo info)
        {
            this.info = info;
        }


        public void LoadPlayerPalettes(WorldRenderer wr,string playerName,HSLColor color,bool replaceExisting)
        {
            var basePalette = wr.Palette(info.BasePalette).Palette;
            ImmutablePalette pal;
            int[] remap;

            if (info.PlayerIndex.TryGetValue(playerName, out remap))
                pal = new ImmutablePalette(basePalette, new IndexedColorRemap(basePalette, info.RemapIndex, remap));
            else
                pal = new ImmutablePalette(basePalette);

            wr.AddPalette(info.BaseName + playerName, pal, info.AllowModifiers, replaceExisting);
        }
    }


    public class IndexedColorRemap : IPaletteRemap
    {
        Dictionary<int, int> replacements = new Dictionary<int, int>();
        IPalette basePalette;

        public IndexedColorRemap(IPalette basePalette,int[] ramp,int[] remap)
        {
            this.basePalette = basePalette;
            if (ramp.Length != remap.Length)
                throw new InvalidDataException("ramp and remap lengths do no match.");

            for (var i = 0; i < ramp.Length; i++)
                replacements[ramp[i]] = remap[i];
        }

        public Color GetRemappedColor(Color original,int index)
        {
            int c;
            return replacements.TryGetValue(index, out c) ? basePalette.GetColor(c) : original;
        }
    }
}